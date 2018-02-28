mp.events.add('setMarkerZoneRouteVisible', (blip, visible, color) => {
    blip.setRouteColour(color);
    blip.setRoute(visible);
})
