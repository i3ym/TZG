const keyInput = document.getElementById("key");

function enter() {
  localStorage.setItem("key", keyInput.value);
  location = "play.html";
}

keyInput.value = localStorage.getItem("key");
