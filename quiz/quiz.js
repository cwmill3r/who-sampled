const KEY = 'AIzaSyD11iA-eBNZjIhZ5FtVJhqTkZEgph6zrXY';
const PLAYLIST = 'PLM5JVEgIGu5pdPh59wO0Q8QaNcCwVtm_M';

// include axios from CDN -- see youtube.html

const youtube = axios.create({
  baseURL: 'https://www.googleapis.com/youtube/v3',
  params: {
    part: 'snippet, contentDetails',
    maxresults: 10,
    key: KEY
  }
});

const quizTemplate = `
  <div class="w3-card-4">
    <div class="w3-container w3-blue">
      <h1>who sampled?</h1>
    </div>

    <div id="video-container" class="w3-container">
      <!-- iFrame will appear here -->
      loading...
    </div>

    <div id="answerContainer" class="w3-container w3-blue">
      <!-- choices will appear here -->
      loading ...
    </div>
  </div>
`;

async function renderQuizQuestions() {
  var response = await youtube.get('/playlistItems', {
    params: {
      playlistId: PLAYLIST
    }
  });
  console.log(response.data.items);
  var videoList = response.data.items;

  videoList.map(function(video) {
    console.log(video.contentDetails.videoId);
    console.log(video.snippet.title);
    document.querySelector('#quiz-container').innerHTML += `
      <div class="w3-card-4">
        <div class="w3-container w3-blue">
          <h1>who sampled?</h1>
        </div>

        <div id="video-container" class="w3-container">
          <!-- iFrame will appear here -->
          <iframe src="https://www.youtube.com/embed/${
            video.contentDetails.videoId
          }" />
        </div>

        <div id="answerContainer" class="w3-container w3-blue">
          <!-- choices will appear here -->
          ${video.snippet.title}
        </div>
      </div>
      < br />
    `;
  });
}

// on window load
document.querySelector('#quiz-button').addEventListener('click', function(e) {
  renderQuizQuestions();
});
