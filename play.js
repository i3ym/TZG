const minx = 55.96792;
const miny = 92.794119;
const maxx = 56.092132;
const maxy = 93.039045;

const mapStyles = [
  {
    elementType: "geometry",
    stylers: [{ color: "#242f3e" }],
  },
  {
    elementType: "labels.text.stroke",
    stylers: [{ color: "#242f3e" }],
  },
  {
    elementType: "labels.text.fill",
    stylers: [{ color: "#746855" }],
  },
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

const endButton = document.getElementById("endButton");
const fullscreenButton = document.getElementById("fullscreenButton");
const mapWrapper = document.getElementById("mapWrapper");
const mapDiv = document.getElementById("map");
const streetPanoramaDiv = document.getElementById("streetPanorama");

let expectedMarker, targetMarker, polylineMarker;
let streetService;
let streetPanorama;
let map;
let position;
let gameStarted;
let mouseCaptured;

function start(key) {
  // firefox fix
  navigator.__defineGetter__("userAgent", function () {
    return "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
  });

  const script = document.createElement("script");
  script.src = `https://maps.googleapis.com/maps/api/js?key=${key}&v=3&callback=initializeMaps&v=weekly`;
  script.async = true;
  document.head.appendChild(script);
}

function setButtonSvgPath(button, path) {
  button.childNodes[1].childNodes[1].setAttribute("d", path);
}

async function initializeMaps() {
  map = new google.maps.Map(mapDiv, {
    clickableIcons: false,
    fullscreenControl: false,
    mapTypeControl: false,
    streetViewControl: false,
    zoomControl: false,
    styles: mapStyles,
  });
  streetService = new google.maps.StreetViewService();
  streetPanorama = new google.maps.StreetViewPanorama(streetPanoramaDiv, {
    addressControl: false,
    showRoadLabels: false,
    zoomControl: false,
    enableCloseButton: false,
    fullscreenControl: false,
  });
  expectedMarker = new google.maps.Marker({
    map,
    draggable: true,
    visible: false,
  });
  targetMarker = new google.maps.Marker({
    map: map,
    visible: false,
  });
  polylineMarker = new google.maps.Polyline({
    geodesic: true,
    strokeColor: "#FF0000",
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map,
    visible: false,
  });

  map.setCenter({ lat: minx + (maxx - minx), lng: miny + (maxy - miny) });
  map.setZoom(12);
  map.addListener("click", onMapClick);

  streetPanoramaDiv.addEventListener("mousedown", () => {
    mouseCaptured = true;
    mapWrapper.classList.add("minimized");
  });

  document.addEventListener("mouseup", () => {
    mouseCaptured = false;
  });

  mapDiv.addEventListener("mouseenter", () => {
    if (!mouseCaptured) mapWrapper.classList.remove("minimized");
  });

  await startGame();
}

function onMapClick(e) {
  if (!gameStarted) return;

  if (!expectedMarker.getVisible()) {
    endButton.classList.add("visible");
    expectedMarker.setVisible(true);
  }

  expectedMarker.setPosition(e.latLng);
}

function onEndButtonClick() {
  if (gameStarted) endGame();
  else restartGame();
}

function onFullscreenButtonClick() {
  if (document.fullscreenElement) {
    setButtonSvgPath(
      fullscreenButton,
      "M0 0v6h2V2h4V0H0zm16 0h-4v2h4v4h2V0h-2zm0 16h-4v2h6v-6h-2v4zM2 12H0v6h6v-2H2v-4z"
    );
    document.exitFullscreen();
  } else {
    setButtonSvgPath(
      fullscreenButton,
      "M4 4H0v2h6V0H4v4zm10 0V0h-2v6h6V4h-4zm-2 14h2v-4h4v-2h-6v6zM0 14h4v4h2v-6H0v2z"
    );
    document.body.requestFullscreen();
  }
}

async function startGame() {
  if (gameStarted) return;

  gameStarted = true;

  for (let i = 0; i < 3; i++) {
    const posX = Math.random() * (maxx - minx) + minx;
    const posY = Math.random() * (maxy - miny) + miny;

    try {
      const panorama = await streetService.getPanorama({
        location: { lat: posX, lng: posY },
        radius: 1000,
        preference: "nearest",
        source: "outdoor",
      });
      position = panorama.data.location.latLng;

      break;
    } catch {}
  }

  streetPanorama.setPosition(position);

  setButtonSvgPath(
    endButton,
    "M480 128c0 8-3 16-9 23L215 407a32 32 0 0 1-46 0L41 279c-6-7-9-15-9-23a32 32 0 0 1 55-23l105 106 233-234c7-6 15-9 23-9 17 0 32 14 32 32z"
  );
}

async function restartGame() {
  expectedMarker.setDraggable(true);
  expectedMarker.setVisible(false);
  targetMarker.setVisible(false);
  polylineMarker.setVisible(false);

  endButton.classList.remove("visible");
  mapWrapper.classList.remove("maximized");

  await startGame();
}

function endGame() {
  if (!gameStarted || !expectedMarker.getVisible()) return;

  gameStarted = false;

  setButtonSvgPath(
    endButton,
    "M460 71 288 214v84l172 143c20 17 52 2 52-26V96c0-27-32-42-52-25zm-256 0L11 231a32 32 0 0 0 0 50l192 159c21 17 53 2 53-25V95c-1-26-32-41-52-24z"
  );
  mapWrapper.classList.add("maximized");

  const expectedPosition = expectedMarker.getPosition();

  expectedMarker.setDraggable(false);
  targetMarker.setPosition(position);
  targetMarker.setVisible(true);
  polylineMarker.setPath([expectedPosition, position]);
  polylineMarker.setVisible(true);

  let minPosX = expectedPosition.lat();
  let maxPosX = position.lat();
  let minPosY = expectedPosition.lng();
  let maxPosY = position.lng();

  if (minPosX > maxPosX) [minPosX, maxPosX] = [maxPosX, minPosX];
  if (minPosY > maxPosY) [minPosY, maxPosY] = [maxPosY, minPosY];

  const bounds = new google.maps.LatLngBounds(
    { lat: minPosX, lng: minPosY },
    { lat: maxPosX, lng: maxPosY }
  );

  map.fitBounds(bounds, 2);
}

function returnToStart() {
  streetPanorama.setPosition(position);
}

const savedkey = localStorage.getItem("key");
if (!savedkey) location = "index.html";
else start(savedkey);
