

// Global reference storage
window._dartboardRef = null;

function setDartboardReference(dotNetRef) {
    window._dartboardRef = dotNetRef;
    console.log('Dartboard reference set:', dotNetRef);
}

function dartboardClick(value, segmentType) {
    console.log('Dartboard clicked:', value, segmentType);
    if (window._dartboardRef) {
        window._dartboardRef.invokeMethodAsync('HandleSegmentClick', value, segmentType)
            .then(() => console.log('Click handled successfully'))
            .catch(err => console.error('Click failed:', err));
    } else {
        console.error('No dartboard reference set!');
    }
}
