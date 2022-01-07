const minx = 55.967920;
const miny = 92.794119;
const maxx = 56.092132;
const maxy = 93.039045;

const style = document.createElement('style');
document.body.appendChild(style);

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
    location = 'index.js';
}

async function startGame() {
    style.textContent = `
        #mapdiv {
            height: 500px;
            width: 500px;
            bottom: 10px;
            right: 80px;
        }
        #mapdiv:hover {
            height: 90%;
            width: 80%;
        }
    `;

    for (const m in markers) m.setMap(null);
    marker?.setMap(null);

    map ||= new google.maps.Map(document.getElementById("map"), {
        clickableIcons: false,
        fullscreenControl: false,
        mapTypeControl: false,
        streetViewControl: false,
        zoomControl: false,
    });
    map.setCenter({ lat: 56.026, lng: 92.94 });
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

    style.textContent = `
        #mapdiv {
            height: 100%;
            width: 100%;
            bottom: 0;
            right: 0;
        }
    `;

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
if (!savedkey) location = 'index.js';
else start(savedkey);