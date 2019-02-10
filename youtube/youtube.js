var KEY = 'AIzaSyD11iA-eBNZjIhZ5FtVJhqTkZEgph6zrXY';

// include axios from CDN -- see youtube.html

var youtube = axios.create({
  baseURL: 'https://www.googleapis.com/youtube/v3',
  params: {
    part: 'snippet',
    maxresults: 5,
    key: KEY
  }
});

async function onTermSubmit(e) {
  e.preventDefault();
  var searchTerm = document.querySelector('#textBox').value;
  console.log(searchTerm);
  var response = await youtube.get('/search', {
    params: {
      q: searchTerm
    }
  });
  console.log(response.data.items);
  var videoList = response.data.items;

  videoList.map(function(video) {
    console.log(video.id.videoId);
    console.log(video.snippet.title);
    document.querySelector('body').innerHTML += `
      <div>
        <div>
          <iframe src="https://www.youtube.com/embed/${video.id.videoId}" />
        </div>
        <div>
          ${video.snippet.title}
        </div>
      </div>
    `;
  });
}

document.querySelector('#searchForm').addEventListener('submit', onTermSubmit);
