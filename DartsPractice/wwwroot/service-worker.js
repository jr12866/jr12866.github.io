const CACHE_NAME = 'darts-practice-v1';
const OFFLINE_URL = '/';

self.addEventListener('install', event => {
    console.info('SW: Installing');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.info('SW: Activating');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.info('SW: Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => {
            console.info('SW: Claiming clients');
            return self.clients.claim();
        })
    );
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;
    
    const url = new URL(event.request.url);
    
    event.respondWith(
        caches.match(event.request).then(response => {
            if (response) {
                console.info('SW: Cache hit:', url.pathname);
                return response;
            }
            
            console.info('SW: Cache miss, fetching:', url.pathname);
            return fetch(event.request).then(networkResponse => {
                if (!networkResponse || networkResponse.status !== 200) {
                    return networkResponse;
                }
                
                if (url.origin === self.location.origin) {
                    const responseToCache = networkResponse.clone();
                    caches.open(CACHE_NAME).then(cache => {
                        cache.put(event.request, responseToCache);
                    });
                }
                
                return networkResponse;
            }).catch(error => {
                console.info('SW: Fetch failed, trying cache for:', url.pathname);
                if (event.request.mode === 'navigate') {
                    return caches.match(OFFLINE_URL).then(response => {
                        if (response) {
                            console.info('SW: Serving offline page');
                            return response;
                        }
                        return new Response('Offline - Please connect to the internet', { 
                            status: 503,
                            headers: { 'Content-Type': 'text/html' }
                        });
                    });
                }
                return new Response('Offline', { status: 503 });
            });
        })
    );
});

self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.info('SW: Received SKIP_WAITING');
        self.skipWaiting().then(() => {
            console.info('SW: skipWaiting complete');
            return self.clients.claim();
        }).then(() => {
            console.info('SW: clients claimed');
            return self.clients.matchAll();
        }).then(clients => {
            clients.forEach(client => {
                client.postMessage({ type: 'RELOAD_PAGE' });
            });
        });
    }
});
