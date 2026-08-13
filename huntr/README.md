# HuntR - Hunting Party Tracker

A mobile-first web app for hunting parties to share real-time positions, chat, and create pins.

**Live URL:** https://huntr.app

## Features
- Create/Join hunting parties with invite codes
- Real-time map with member positions
- Rich pins (observations, landmarks, game sightings)
- Chat messaging
- Hunt archives
- PWA - add to home screen for app-like experience

## Development

Open `index.html` directly in a browser, or serve locally:

```bash
npx serve .
```

## Deployment to GitHub Pages

This app is configured for GitHub Pages deployment:

1. Push this repo to GitHub
2. Go to Settings → Pages
3. Select "Deploy from a branch" → main branch
4. Your app will be live at `https://yourusername.github.io/repo-name`

## PWA Installation

On mobile:
1. Open the app in Safari/Chrome
2. Tap Share → "Add to Home Screen"
3. The app will install like a native app

## Tech

- Vanilla HTML/CSS/JavaScript (no build required)
- Leaflet.js for maps
- OpenTopoMap tiles
- LocalStorage for data persistence
- PWA with manifest and service worker ready
