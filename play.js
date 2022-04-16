const minx = 55.967920;
const miny = 92.794119;
const maxx = 56.092132;
const maxy = 93.039045;

const markers = [];
let marker;
let sv;
let street, map;
let pos;


function start(key) {
    if (!key) localStorage.removeItem('key');
    else localStorage.setItem('key', key);

    const scr = document.createElement('script');
    scr.src = 'https://maps.googleapis.com/maps/api/js?key=' + key + '&callback=startGame&v=weekly';
    scr.async = true;
    document.body.appendChild(scr);
}
function resetKey() {
    location = 'index.html';
}


async function startGame() {
    function getDarkMapStyles() {
        return [
            { elementType: "geometry", stylers: [{ color: "#242f3e" }] },
            { elementType: "labels.text.stroke", stylers: [{ color: "#242f3e" }] },
            { elementType: "labels.text.fill", stylers: [{ color: "#746855" }] },
            {
                featureType: "administrative.locality",
                elementType: "labels.text.fill",
                stylers: [{ color: "#d59563" }],
            },
            {
                featureType: "poi",
                elementType: "labels.text.fill",
                stylers: [{ color: "#d59563" }],
            },
            {
                featureType: "poi.park",
                elementType: "geometry",
                stylers: [{ color: "#263c3f" }],
            },
            {
                featureType: "poi.park",
                elementType: "labels.text.fill",
                stylers: [{ color: "#6b9a76" }],
            },
            {
                featureType: "road",
                elementType: "geometry",
                stylers: [{ color: "#38414e" }],
            },
            {
                featureType: "road",
                elementType: "geometry.stroke",
                stylers: [{ color: "#212a37" }],
            },
            {
                featureType: "road",
                elementType: "labels.text.fill",
                stylers: [{ color: "#9ca5b3" }],
            },
            {
                featureType: "road.highway",
                elementType: "geometry",
                stylers: [{ color: "#746855" }],
            },
            {
                featureType: "road.highway",
                elementType: "geometry.stroke",
                stylers: [{ color: "#1f2835" }],
            },
            {
                featureType: "road.highway",
                elementType: "labels.text.fill",
                stylers: [{ color: "#f3d19c" }],
            },
            {
                featureType: "transit",
                elementType: "geometry",
                stylers: [{ color: "#2f3948" }],
            },
            {
                featureType: "transit.station",
                elementType: "labels.text.fill",
                stylers: [{ color: "#d59563" }],
            },
            {
                featureType: "water",
                elementType: "geometry",
                stylers: [{ color: "#17263c" }],
            },
            {
                featureType: "water",
                elementType: "labels.text.fill",
                stylers: [{ color: "#515c6d" }],
            },
            {
                featureType: "water",
                elementType: "labels.text.stroke",
                stylers: [{ color: "#17263c" }],
            },
        ];
    }

    document.getElementById('endb').childNodes[1].childNodes[1].setAttribute('d', "M480 128c0 8.188-3.125 16.38-9.375 22.62l-256 256C208.4 412.9 200.2 416 192 416s-16.38-3.125-22.62-9.375l-128-128C35.13 272.4 32 264.2 32 256c0-18.28 14.95-32 32-32c8.188 0 16.38 3.125 22.62 9.375L192 338.8l233.4-233.4C431.6 99.13 439.8 96 448 96C465.1 96 480 109.7 480 128z");
    document.getElementById("mapdiv").classList.remove("maximized");

    for (const m in markers) markers[m].setMap(null);
    marker?.setMap(null);
    markers.length = 0;
    marker = null;

    map ||= new google.maps.Map(document.getElementById("map"), {
        clickableIcons: false,
        fullscreenControl: false,
        mapTypeControl: false,
        streetViewControl: false,
        zoomControl: false,
        styles: getDarkMapStyles(),
    });
    map.setCenter({ lat: minx + (maxx - minx), lng: miny + (maxy - miny) });
    map.setZoom(12);

    function addMarker(e) {
        marker ||= new google.maps.Marker({ map: map, draggable: true, });
        marker.setPosition(e.latLng);
    }
    map.addListener("click", addMarker);
    sv ||= new google.maps.StreetViewService();

    for (let i = 0; i < 3; i++) {
        const posx = Math.random() * (maxx - minx) + minx;
        const posy = Math.random() * (maxy - miny) + miny;
        pos = { lat: posx, lng: posy };

        try {
            const p = await sv.getPanorama({
                location: pos,
                radius: 1000,
                preference: "nearest",
                source: "outdoor",
            });
            pos = p.data.location.latLng;

            break;
        }
        catch { }
    }

    street ||= new google.maps.StreetViewPanorama(document.getElementById("street"), {
        addressControl: false,
        showRoadLabels: false,
        zoomControl: false,
    });
    street.setPosition(pos);
}
function endGame() {
    if (!marker) return;
    if (markers.length != 0) {
        startGame();
        return;
    }

    document.getElementById('endb').childNodes[1].childNodes[1].setAttribute('d', "M459.5 71.41l-171.5 142.9v83.45l171.5 142.9C480.1 457.7 512 443.3 512 415.1V96.03C512 68.66 480.1 54.28 459.5 71.41zM203.5 71.41L11.44 231.4c-15.25 12.87-15.25 36.37 0 49.24l192 159.1c20.63 17.12 52.51 2.749 52.51-24.62v-319.9C255.1 68.66 224.1 54.28 203.5 71.41z");
    document.getElementById("mapdiv").classList.add("maximized");

    const mappos = marker.getPosition();
    const streetpos = pos;
    const difflat = mappos.lat() - streetpos.lat();
    const difflng = mappos.lng() - streetpos.lng();

    markers.push(new google.maps.Marker({
        position: streetpos,
        map: map,
    }));

    markers.push(new google.maps.Polyline({
        path: [mappos, streetpos],
        geodesic: true,
        strokeColor: "#FF0000",
        strokeOpacity: 1.0,
        strokeWeight: 2,
        map: map,
    }));
}
function retToStart() {
    street.setPosition(pos);
}



const savedkey = localStorage.getItem('key');
if (!savedkey) location = 'index.html';
else start(savedkey);