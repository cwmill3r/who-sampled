var KEY = 'AIzaSyD11iA-eBNZjIhZ5FtVJhqTkZEgph6zrXY';

// include axios from CDN -- see youtube.html

axios.create({
  baseURL: 'https://www.googleapis.com/youtube/v3',
  params: {
    part: 'snippet',
    maxresults: 5,
    key: KEY
  }
});

function onTermSubmit() {
  console.log('submitted');
}

document.querySelector('#searchForm').addEventListener('submit', onTermSubmit);
