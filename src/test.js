import icon from './assets/icon.png';
export default function hello() {
    const message = 'just making sure you work';
    document.querySelector('#test').innerHTML += `<br /> ${message}`;
}
