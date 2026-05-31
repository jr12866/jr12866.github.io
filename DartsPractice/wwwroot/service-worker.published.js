self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.info('SW: Received SKIP_WAITING');
        self.skipWaiting().then(() => {
            console.info('SW: skipWaiting complete, claiming clients');
            return self.clients.claim();
        }).then(() => {
            console.info('SW: clients claimed, notifying all clients to reload');
            return self.clients.matchAll();
        }).then(clients => {
            clients.forEach(client => {
                client.postMessage({ type: 'SW_ACTIVATED' });
            });
        }).catch(err => {
            console.error('SW: Error in skipWaiting chain:', err);
        });
    }
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html$/, /\.js$/, /\.json$/, /\.css$/, /\.woff2?$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/, /\.woff2$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => {
            const url = new URL(asset.url, baseUrl).href;
            return new Request(url, { 
                integrity: asset.hash, 
                cache: 'no-cache',
                credentials: 'same-origin'
            });
        });

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
    await self.skipWaiting();
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    const cacheKeys = await caches.keys();
    await Promise.all(
        cacheKeys
            .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
            .map(key => caches.delete(key))
    );

    await self.clients.claim();
}

async function onFetch(event) {
    const { request } = event;
    const url = new URL(request.url);

    if (request.method !== 'GET') {
        return fetch(request);
    }

    const shouldServeIndexHtml = request.mode === 'navigate'
        && !manifestUrlList.some(manifestUrl => manifestUrl === request.url);

    let cache;
    try {
        cache = await caches.open(cacheName);
    } catch {
        return fetch(request);
    }

    if (shouldServeIndexHtml) {
        const cachedResponse = await cache.match('index.html');
        if (cachedResponse) {
            return cachedResponse;
        }
        try {
            const networkResponse = await fetch(request);
            if (networkResponse.ok) {
                cache.put(request, networkResponse.clone());
            }
            return networkResponse;
        } catch {
            const cachedResponse = await cache.match('index.html');
            if (cachedResponse) {
                return cachedResponse;
            }
        }
        return new Response('Offline - Unable to load page', { status: 503 });
    }

    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch {
        if (request.destination === 'document') {
            const cachedResponse = await cache.match('index.html');
            if (cachedResponse) {
                return cachedResponse;
            }
        }
        return new Response('Offline - Resource unavailable', { status: 503 });
    }
}
