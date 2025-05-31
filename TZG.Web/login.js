const keyInput = document.getElementById("key");

function enter() {
  localStorage.setItem("key", keyInput.value);
  location = "play.html";
}

const key = localStorage.getItem("key");

if (!key) {
  localStorage.setItem("key", "CrocodiloBombardino");
  location = "play.html";
}

keyInput.value = key;
