



const keyinput = document.getElementById('keyinput');
keyinput.value = localStorage.getItem('key');


function enter() {
    localStorage.setItem('key', keyinput.value);
    location = 'play.html';
}