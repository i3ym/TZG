const minx = 55.967920;
const miny = 92.794119;
const maxx = 56.092132;
const maxy = 93.039045;

let sv;
let street, map;
let marker;
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
    location = 'index.js';
}

async function startGame() {
    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: 56.026, lng: 92.94 },
        zoom: 16,
        clickableIcons: false,
        fullscreenControl: false,
        mapTypeControl: false,
        streetViewControl: false,
    });

    function addMarker(e) {
        marker ||= new google.maps.Marker({ map: map, draggable: true, });
        marker.setPosition(e.latLng);

        console.log('+m');
    }
    map.addListener("click", addMarker);

    let posx, posy;
    sv ||= new google.maps.StreetViewService();

    for (let i = 0; i < 3; i++) {
        const posx = Math.random() * (maxx - minx) + minx;
        const posy = Math.random() * (maxy - miny) + miny;
        pos = { lat: posx, lng: posy };

        const p = await sv.getPanorama({
            location: pos,
            radius: 1000,
            preference: "nearest",
            source: "outdoor",
        });
        pos = p.data.location.latLng;
    }

    street = new google.maps.StreetViewPanorama(document.getElementById("street"), {
        position: pos,
        addressControl: false,
        showRoadLabels: false,
    });
}
function retToStart() {
    street.setPosition(pos);
}
function endGame() {
    if (!marker) return;

    const mappos = marker.getPosition();
    const streetpos = pos;
    const difflat = mappos.lat() - streetpos.lat();
    const difflng = mappos.lng() - streetpos.lng();

    new google.maps.Marker({
        position: streetpos,
        map: map,
    });

    new google.maps.Polyline({
        path: [mappos, streetpos],
        geodesic: true,
        strokeColor: "#FF0000",
        strokeOpacity: 1.0,
        strokeWeight: 2,
        map: map,
    });
}



const savedkey = localStorage.getItem('key');
if (!savedkey) location = 'index.js';
else start(savedkey);