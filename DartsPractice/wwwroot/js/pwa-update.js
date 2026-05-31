window.pwaUpdate = {
    registration: null,

    init: function () {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then((reg) => {
                this.registration = reg;
            }).catch((err) => {
                console.error('SW ready error:', err);
            });
        }
    },

    notifyUpdate: function () {
        if (window.pwaUpdate.dotNetRef) {
            window.pwaUpdate.dotNetRef.invokeMethod('NotifyUpdateAvailable');
        }
    },

    initWithCallback: function (dotNetRef) {
        window.pwaUpdate.dotNetRef = dotNetRef;
        this.init();
    },

    listenForMessages: function () {
        if ('serviceWorker' in navigator && navigator.serviceWorker) {
            navigator.serviceWorker.addEventListener('message', (event) => {
                if (event.data && event.data.type === 'RELOAD_PAGE') {
                    window.location.reload();
                }
            });
        }
    },

    watchForUpdates: function () {
        if (!this.registration) return;

        this.registration.addEventListener('updatefound', () => {
            const newWorker = this.registration.installing;
            if (newWorker) {
                newWorker.addEventListener('statechange', (e) => {
                    if (e.target.state === 'installed') {
                        if (this.registration.waiting && navigator.serviceWorker.controller) {
                            this.notifyUpdate();
                        }
                    }
                });
            }
        });
    },

    applyUpdate: function () {
        if (!this.registration || !this.registration.waiting) {
            return false;
        }
        this.registration.waiting.postMessage({ type: 'SKIP_WAITING' });
        return true;
    },

    checkForUpdates: function () {
        if (this.registration) {
            this.registration.update();
        }
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        if (!window.pwaUpdate.dotNetRef) {
            window.pwaUpdate.init();
        }
    });
} else {
    if (!window.pwaUpdate.dotNetRef) {
        window.pwaUpdate.init();
    }
}
