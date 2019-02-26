// WHO SAMPLED is a JS and C# web app which allows users
// to submit quiz questions and allows them to play a quiz
// testing their knowledge of HipHop samples

// Below there are rougly 3 secions of code
// 1. functions to render content and handle button clicks and form submits i.e. handleClick() and renderSection()
// 2. Utility functions which call the web services
// 3. event listeners to trigger the methods named handle*Whatever*

// This project must be run in Visual Studio to take advantage of the Services we wrote in C#
// It utilizes a MySql db that is currently hosted on Plesk

// GLOBAL Variables
let quizQuestions = undefined;
let userInfo = undefined;
let userQuestions = undefined;
let questionCounter = undefined;

// quiz global variables ... remember to clear them when the quiz is finished
let quizPanels = []; // this is for hiding and showing :(
let totalQuizQuestions = undefined;
let rightAnswers = undefined;

// content panels used in hiding showing
const contentPanels = [
  'homeTab',
  'aboutTab',
  'accountTab',
  'addQuestionTab',
  'accountSettingsTab',
  'createAccountTab',
  'createQuestionTab',
  'quizTab',
  'editQuestionTab',
  'finalScoreTab' 
];

// There are also panels for the quiz found below
 
// magic show panel function
function showPanel(panelId) {
  for (var i = 0; i < contentPanels.length; i++) {
    if (panelId == contentPanels[i]) {
      $('#' + contentPanels[i]).css('visibility', 'visible');
    } else {
      $('#' + contentPanels[i]).css('visibility', 'hidden');
    }
  }
}

// magic show panel function for the quiz questions
function showQuizPanel(panelId) {
  // let the parent container show
  for (var i = 0; i < quizPanels.length; i++) {
    if (panelId == quizPanels[i]) {
      $('#' + quizPanels[i]).css('visibility', 'visible');
    } else {
      $('#' + quizPanels[i]).css('visibility', 'hidden');
    }
  }
  // keep the parent showing visible
  document.querySelector('#quizTab').style.visibility = "visible";
}

// login form submit handler
function handleLoginFormSubmit(e) {
  e.preventDefault();
  const id = document.querySelector('#logonId').value;
  const pass = document.querySelector('#logonPassword').value;
  LogOn(id, pass);
  return false;
}

function clearLogOnForm() {
  document.querySelector('#logonId').value = "";
  document.querySelector('#logonPassword').value = "";
}

// create account form submit handler
function handleCreateAccountFormSubmit(e) {
  e.preventDefault();
  const username = document.querySelector('#ca-username').value;
  const password = document.querySelector('#ca-password').value;
  const firstname = document.querySelector('#ca-firstname').value;
  const lastname = document.querySelector('#ca-lastname').value;
  const email = document.querySelector('#ca-email').value;
  // actually create the account
  CreateAccount(username, password, firstname, lastname, email);
}
 
function renderAccountSettingsPage(userInfo) {
  // render the heading and greeting of the account settings page
  document.querySelector('#accountSettingsTab').innerHTML = `
  <div class="w3-container">
    <img src="http://i.pravatar.cc/300" class="w3-bar-item w3-circle w3-hide-small" style="width:85px">
    <h3 class="w3-text-teal">hello ${userInfo.userId}</h3>
    <a style="text-decoration: underline; cursor: pointer;" class="w3-text-indigo" onClick="handleLogoffClick()" id="logoff-button">Logoff</a>
    <div class="w3-right">
      <button onclick="handleCreateQuestionClick(this)" id="createQuestionButton">create new question</button>
    </div>
  </div>
  <div class="w3-container">
    <div class="w3-section">
      <ul id="userQuestionsList" class="w3-ul w3-card-4 w3-white"></ul>
    </div>
  </div>
  </br>
`
  // render the questions that the user created
  GetUserCreatedQuestions(userInfo.id); 
}

// this renders the questions that a user created
function renderUserCreatedQuestions(userQuestions) {
  document.querySelector('#accountSettingsTab').innerHTML += `
  <div id="editQuestionsContainer">
    <ul id="userQuestionsList" class="w3-ul w3-card-4"></ul>
  </div>
  `
  document.querySelector('#userQuestionsList').innerHTML = "";
  // Could do some cooler stuff here but right now it's fairly basic
  userQuestions.map(function (question, index) {
    console.log(question);
    document.querySelector('#userQuestionsList').innerHTML += `
      <li class="w3-bar">
        <span data-questionId=${question.questionId} onclick="handleDeleteQuestionClick(this)" class="questionDeleteButton w3-bar-item w3-button w3-xlarge w3-right">
          <i data-questionId=${question.questionId} class="questionDeleteButton fa fa-times" aria-hidden="true"></i>
          <p data-questionId=${question.questionId} class="questionDeleteButton w3-small">delete</p>
        </span>
        <span data-questionId=${question.questionId} onclick="handleEditQuestionClick(this)" class="questionEditButton w3-bar-item w3-button w3-xlarge w3-right">
          <i data-questionId=${question.questionId} onclick="handleEditQuestionClick(this)" class="fa fa-pencil" aria-hidden="true"></i>
          <p data-questionId=${question.questionId} onclick="handleEditQuestionClick(this)" class="w3-small">edit</p>
        </span>
   
        <div class="w3-bar-item">
          <iframe src="${question.videoId}"></iframe>
          <span class="w3-large w3-text-grey"> ... sampled on ${question.correctAnswerText}</span><br>
        </div>
      </li>
    `
  });
}

function handleCreateQuestionClick(e) {
  //clear the questions list
  document.querySelector('#userQuestionsList').innerHTML = "";
  showPanel('createQuestionTab');
}

function handleDeleteQuestionClick(e) {
  const questionId = e.getAttribute("data-questionId");
  console.log(`questionId is ${questionId}`);
  DeleteQuestion(questionId);
}

function handleEditQuestionClick(event) { 
  const questionId = event.getAttribute("data-questionId");
  // first we render the form filled with that questions stuff
  renderEditQuestionForm(questionId);
}

function handleEditQuestionFormSubmit(e) {
  e.preventDefault();
  const questionId = document.querySelector('#eq-questionId').value;
  const questionText = document.querySelector('#eq-questionText').value;
  const sampleYouTubeLink = document.querySelector('#eq-sampleYouTubeLink').value;
  const correctAnswerText = document.querySelector('#eq-correctAnswerText').value;
  const wrongAnswer1 = document.querySelector('#eq-wrongAnswer1').value;
  const wrongAnswer2 = document.querySelector('#eq-wrongAnswer2').value;
  const wrongAnswer3 = document.querySelector('#eq-wrongAnswer3').value;
  EditQuestion(questionId, questionText, sampleYouTubeLink, correctAnswerText, wrongAnswer1, wrongAnswer2, wrongAnswer3);
  document.querySelector('#editQuestionTab').innerHTML = "";
}

function renderEditQuestionForm(questionId) {
  showPanel('editQuestionTab');
  console.log(userQuestions);
  console.log(questionId);

  // if anyone is reading this I am proud of this little function chain ;)
  userQuestions.filter(function (q) {
    console.log(q);
    return q.questionId == questionId;
  }).map(function (q) {
    document.querySelector('#eq-questionText').value = q.questionText;
    document.querySelector('#eq-sampleYouTubeLink').value = q.videoId;
    document.querySelector('#eq-correctAnswerText').value = q.correctAnswerText;
    document.querySelector('#eq-questionId').value = q.questionId; // written to the hidden input
    q.wrongAnswers.map(function (w, index) {
      document.querySelector(`#eq-wrongAnswer${index+1}`).value = w.wrongAnswerText;
    })
  });
  
}

function handleLogoffClick() {
  LogOff();
  document.querySelector('#accountSettingsTab').innerHTML = "";
}

function handleCreateQuestionFormSubmit(e) {
  e.preventDefault();
  console.log(userInfo.id);
  const creatorIdToPass = userInfo.id;
  const questionText = document.querySelector('#cq-questionText').value;
  const sampleTrackName = document.querySelector('#cq-sampleTrackName').value;
  const sampleArtistName = document.querySelector('#cq-sampleArtistName').value;
  const sampleYouTubeLink = document.querySelector('#cq-sampleYouTubeLink').value;
  const songArtistName = document.querySelector('#cq-songArtistName').value;
  const songTitle = document.querySelector('#cq-songTitle').value;
  const wrongAnswer1 = document.querySelector('#cq-wrongAnswer1').value;
  const wrongAnswer2 = document.querySelector('#cq-wrongAnswer2').value;
  const wrongAnswer3 = document.querySelector('#cq-wrongAnswer3').value;
  //alert('form about to be submitted');
  // actually create the account
  console.log(creatorIdToPass + " " + questionText + " " + sampleTrackName + " " + sampleArtistName + " " + sampleYouTubeLink + " " + songArtistName + " " + songTitle + " " + wrongAnswer1 + " " + wrongAnswer2 + " " + wrongAnswer3)
  CreateQuestion(creatorIdToPass, questionText, sampleTrackName, sampleArtistName, sampleYouTubeLink, songArtistName, songTitle, wrongAnswer1, wrongAnswer2, wrongAnswer3);
}

// this function is kind of complicated but it maps over the quizQuestions and their wrong answers
// it creates an array of their buttons which are the answer choices and finally...
// it shuffles them for each question giving an element of randomness
function renderQuestions() {
  // let the global quizQuestions variable know how many questions in THIS quiz
  totalQuizQuestions = quizQuestions.length;
  // the first time through right answers is undefined so we set it to zero
  // right answers are added in the right and wrong answers click event listeners
  rightAnswers = 0;
  questionCounter = 0;
  const quizDiv = document.querySelector('#quizTab');
  quizQuestions.map(function (q, index) {
    quizPanels.push(`quizPanel-${index}`);
      quizDiv.innerHTML += `
        <div class="w3-container w3-display-middle w3-twothird w3-card w3-padding-large w3-white" style="visibility: hidden;" id="quizPanel-${index}">
          <h3>
            Question ${index + 1} of ${quizQuestions.length}
          </h3>
          <div id="videoPane">
            <iframe style="width:100%; height:100%; min-height: 16vw;" id="videoiFrame${index}" class="" src="${q.videoId}" frameborder="0" allow="accelerometer; encrypted-media; gyroscope; picture-in-picture;" allowfullscreen></iframe>
          </div>     
          <h1 class="w3-center">
            <b>${q.questionText}</b>
          </h1>
          <div id="buttonDiv-${q.questionId}">
            <!-- Buttons Render Here-->
          </div>
        </div>
      `
    
    let answerArray = [];
    answerArray.push(
      `<button style="font-size: 2vw; display:inline-block; width: 100%; margin-top: 1rem;" class="w3-button w3-teal w3-center" onclick="handleRightAnswerClick(this)" data-quizPanel="quizPanel-${index}" id="${q.questionid}">${q.correctAnswerText}</button>`
    );

    q.wrongAnswers.map(function (w) {
      answerArray.push(`<button style="font-size: 2vw; display:inline-block; width: 100%; margin-top: 1rem;" class="w3-button w3-teal w3-center" onclick="handleWrongAnswerClick(this)" data-quizPanel="quizPanel-${index}" id="${w.questionId}">${w.wrongAnswerText}</button>`);
    });
    let shuffledArray = shuffle(answerArray);

    let specificButtonDiv = document.querySelector(`#buttonDiv-${q.questionId}`)

    shuffledArray.map(function (a) {
      specificButtonDiv.innerHTML += `
        ${a}
      `
    });

  });

  quizPanels.map(function (q) { console.log(q) });
  // show the first question
  console.log(quizPanels[0]);
  showQuizPanel("quizPanel-0");
  // start the first video playing
  //playVideoFunction('videoiFrame-0');
}

// this is the main play button on the homepage
function handlePlayButtonClick(e) {
  e.preventDefault()
  disableNavLinks()
  
  GetQuestions();
  if (quizQuestions != undefined) {
    renderQuestions();
    showPanel('quizTab');

  } else {
    alert('Loading API... hit refresh if no response');
    GetQuestions();
    setTimeout(function () {
      renderQuestions();
      showPanel('quizTab');
    }, 1000)
  }
}

function disableNavLinks() {
  const homeTabLink = document.querySelector('#home-tab-link');
  homeTabLink.disabled = true;
  homeTabLink.innerHTML = "";
  const accountTabLink = document.querySelector('#account-tab-link');
  accountTabLink.disabled = true;
  accountTabLink.innerHTML = "";
}

function enableNavLinks() {
  const homeTabLink = document.querySelector('#home-tab-link');
  homeTabLink.disabled = false;
  homeTabLink.innerHTML = "Home";
  const accountTabLink = document.querySelector('#account-tab-link');
  accountTabLink.disabled = false;
  accountTabLink.innerHTML = "Account";
}

// this and handleWrongAnswerClick could be combined
// but were just doing it this way at first for simplicity
function handleRightAnswerClick(event) {
  if (questionCounter + 1 < totalQuizQuestions) {
    const quizPanelId = event.getAttribute("data-quizpanel");
    //alert('A right answer was clicked so im adding 1 to rightAnswers');
    rightAnswers++;
    questionCounter++;
    // get index of the quizPanel that was clicked
    const indexOfPanelClicked = quizPanels.indexOf(quizPanelId);
    const nextPanel = quizPanels[indexOfPanelClicked + 1];
    // pause the playing video if playing
    stopVideoFunction();
    showQuizPanel(nextPanel);
  } else {
    document.querySelector('#quizTab').innerHTML = "";
    //alert('were gonna show the final score here')
    // calculate the score as a percent and display
    renderFinalScoreCard();
    showPanel('finalScoreTab');
    clearScoreCard();
    enableNavLinks();
  };
  
}

function handleWrongAnswerClick(event) {
  if (questionCounter + 1 < totalQuizQuestions) {
    //alert(questionCounter);
    // keep playing - not last question
    //alert('were gonna keep playing');
    const quizPanelId = event.getAttribute("data-quizpanel");
    //alert('A wrong answer was clicked so im not adding to rightAnswers');
    questionCounter++;
    stopVideoFunction();
    // get index of the quizPanel that was clicked
    const indexOfPanelClicked = quizPanels.indexOf(quizPanelId);
    const nextPanel = quizPanels[indexOfPanelClicked + 1];
    showQuizPanel(nextPanel);
  } else {
    document.querySelector('#quizTab').innerHTML = "";
    //alert('were gonna show the final score here');
    // calculate the score as a percent and display
    renderFinalScoreCard();
    showPanel('finalScoreTab');
    clearScoreCard();
    enableNavLinks();
  }
}

// clears the final score card after 5 seconds 5000 ms
function clearScoreCard() {
  setTimeout(function () {
    const finalScoreTab = document.querySelector('#finalScoreTab');
    finalScoreTab.innerHTML = "";
    showPanel('homeTab');
  }, 5000)

}

function renderFinalScoreCard() {
  setTimeout(function () {
    //document.querySelector('#finalScoreGoesHere').innerHTML = calculateScore();
    const finalScoreTab = document.querySelector('#finalScoreTab');
    finalScoreTab.style.display = 'block';
    finalScoreTab.style.height = '100vh';
    finalScoreTab.style.width = '100vw';
    finalScoreTab.style.paddingTop = '0';
    const finalScoreCard = document.querySelector('#finalScoreCard');
    //finalScoreCard.style.paddingTop = '0';
    finalScoreCard.style.height = '100vh';
    finalScoreCard.style.width = '100vw';
    finalScoreCard.style.backgroundColor = 'black';
    finalScoreCard.style.color = 'pink';
    const finalScoreMessage = document.querySelector('#finalScoreMessage');
    finalScoreMessage.innerHTML =
      `Thanks for playing WHO SAMPLED...` + `Your score is ${calculateScore()}`;
  }, 1000)
};

function calculateScore() {
  const finalPercent = rightAnswers / totalQuizQuestions;
  return finalPercent.toLocaleString('en', { style: "percent" });
}

function playVideoFunction(id) {
  document.getElementById(id).src += "&autoplay=1";
}

function stopVideoFunction() {
  var videoText = "videoiFrame" + (questionCounter - 1).toString();
  console.log(videoText)
  var ysrc = document.getElementById(videoText).src;

  var newsrc = ysrc.replace("&autoplay=1", "");
  document.getElementById(videoText).src = newsrc;
}

// Fisher-Yates shuffle algorithm https://github.com/coolaj86/knuth-shuffle
function shuffle(array) {
  var currentIndex = array.length, temporaryValue, randomIndex;

  // While there remain elements to shuffle...
  while (0 !== currentIndex) {

    // Pick a remaining element...
    randomIndex = Math.floor(Math.random() * currentIndex);
    currentIndex -= 1;

    // And swap it with the current element.
    temporaryValue = array[currentIndex];
    array[currentIndex] = array[randomIndex];
    array[randomIndex] = temporaryValue;
  }

  return array;
}

// BELOW ARE API UTILITIY TYPE THINGS

// Utilizes the GetQuestions C# Web Service
function GetQuestions() {
  var webMethod = 'AccountServices.asmx/GetAllQuestions';
  $.ajax({
    type: 'POST',
    url: webMethod,
    //data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function (msg) {
      console.log(msg.d);
      quizQuestions = msg.d;
      return msg.d;
    },
    error: function (e) {
      alert('Error getting questing from API');
    }
  });
}

// Utilizes the GetUserCreatedQuestions C# Web Service
function GetUserCreatedQuestions(id) {
  var webMethod = 'AccountServices.asmx/GetUserCreatedQuestions';
  var parameters = `{ id : ${encodeURI(id)}}`;
  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function (msg) {
      console.log(msg.d);
      userQuestions = msg.d;
      renderUserCreatedQuestions(msg.d);
    },
    error: function (e) {
      alert('Error getting users questions from API');
    }
  });
}

// Utilizes the DeleteQuestion C# Web Service
function DeleteQuestion(questionid) {
  var webMethod = 'AccountServices.asmx/DeleteQuestion';
  var parameters = `{ "questionid" : ${encodeURI(questionid)}}`;
  console.log(parameters);
  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function (msg) {
      console.log(msg.d);
      renderAccountSettingsPage(userInfo);
      renderUserCreatedQuestions(userQuestions);
      return msg.d;
    },
    error: function (e) {
      alert('Error deleting question');
    }
  });
}

// Utilizes the CreateQuestion C# Web Service
function CreateAccount(id, pass, fname, lname, email) {
  var webMethod = 'AccountServices.asmx/RequestAccount';
  var parameters =
    '{"uid":"' +
    encodeURI(id) +
    '","pass":"' +
    encodeURI(pass) +
    '","firstName":"' +
    encodeURI(fname) +
    '","lastName":"' +
    encodeURI(lname) +
    '","email":"' +
    encodeURI(email) +
    '"}';

  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function(msg) {
      showPanel('accountTab');
      alert('Congrats your accout was approved LOG IN WITH YOUR NEW CREDENTIALS :)');
    },
    error: function(e) {
      alert('boo...');
    }
  });
}

// Utilizes the CreateQuestion C# Web Service
function CreateQuestion(creatorId, questionText, sampleTrackName, sampleArtistName, sampleYouTubeLink, songArtistName, songTitle, wrongAnswer1, wrongAnswer2, wrongAnswer3, wrongAnswer4) {
  var webMethod = 'AccountServices.asmx/CreateQuestion';
  var parameters = `{
    "creatorId": ${encodeURI(creatorId)},
    "questionText": "${encodeURI(questionText)}",
    "sampleTrackName" : "${encodeURI(sampleTrackName)}",
    "sampleArtistName" : "${encodeURI(sampleArtistName)}",
    "sampleYouTubeLink" : "${encodeURI(sampleYouTubeLink)}",
    "songArtistName" : "${encodeURI(songArtistName)}",
    "songTitle" : "${encodeURI(songTitle)}",
    "wrongAnswer1" : "${encodeURI(wrongAnswer1)}",
    "wrongAnswer2" : "${encodeURI(wrongAnswer2)}",
    "wrongAnswer3" : "${encodeURI(wrongAnswer3)}"
  }`;
  console.log(parameters);  
  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function (msg) {
      alert('Congrats your question was approved...');
      document.querySelector('#accountSettingsTab').innerHTML = "";
      showPanel('accountSettingsTab');
      renderAccountSettingsPage(userInfo);
      renderUserCreatedQuestions(userQuestions);
    },
    error: function (e) {
      alert('boo...');
    }
  });
}

// Utilizes the EditQuestion C# Web Service
function EditQuestion(questionId, questionText, sampleYouTubeLink, correctAnswerText, wrongAnswer1, wrongAnswer2, wrongAnswer3) {
  var webMethod = 'AccountServices.asmx/EditQuestion';
  var parameters = `{
    "questionId": ${questionId},
    "questionText": "${encodeURI(questionText)}",
    "sampleYouTubeLink" : "${encodeURI(sampleYouTubeLink)}",
    "correctAnswerText" : "${encodeURI(correctAnswerText)}",
    "wrongAnswer1" : "${encodeURI(wrongAnswer1)}",
    "wrongAnswer2" : "${encodeURI(wrongAnswer2)}",
    "wrongAnswer3" : "${encodeURI(wrongAnswer3)}"
  }`;
  console.log(parameters);
  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function (msg) {
      // clear some old stuff out to make room...
      //document.querySelector('#accountSettingsTab').innerHTML = "";
      document.querySelector('#userQuestionsList').innerHTML = "";
      // render the new page with the edited question
      renderAccountSettingsPage(userInfo);
      renderUserCreatedQuestions(userQuestions);
      showPanel('accountSettingsTab');
    },
    error: function (e) {
      alert('boo...');
    }
  });
}

// Utilizes the LogOn C# Web Service
function LogOn(userId, pass) {
  var webMethod = 'AccountServices.asmx/LogOn';
  var parameters =
    '{"uid":"' + encodeURI(userId) + '","pass":"' + encodeURI(pass) + '"}';

  $.ajax({
    type: 'POST',
    url: webMethod,
    data: parameters,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function(msg) {
      if (msg.d.loggedIn) {
        showPanel('accountSettingsTab');
        userInfo = msg.d;
        console.log(userInfo); // remove for production
        clearLogOnForm();
        renderAccountSettingsPage(userInfo);
        return true;
      } else {
        //server replied false, so let the user know
        //the logon failed
        alert('logon failed');
        return false;
      }
    },
    error: function(e) {
      alert('boo...');
    }
  });
}

//logs the user off both at the client and at the server
function LogOff() {
  var webMethod = 'AccountServices.asmx/LogOff';
  $.ajax({
    type: 'POST',
    url: webMethod,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    success: function(msg) {
      if (msg.d) {
        showPanel('accountTab');
        userInfo = undefined;
      } else {
        alert('something went wrong with log off');
      }
    },
    error: function(e) {
      alert('boo...');
    }
  });
}

// EVENT LISTENER SECTION

// quiz button click event listener
document.querySelector('#playButton').addEventListener('click', function (e) {
  handlePlayButtonClick(e);
})

// edit question submit event listener
document.querySelector('#editQuestionForm').addEventListener('submit', function (e) {
  handleEditQuestionFormSubmit(e);
});

// create question submit event listener
document.querySelector('#create-question-form').addEventListener('submit', function (e) {
  console.log(e);
  handleCreateQuestionFormSubmit(e);
});

// create account event listener
document
  .querySelector('#create-account-link')
  .addEventListener('click', function (e) {
    e.preventDefault();
    showPanel('createAccountTab');
  });

//// logoff button event listener
//document.querySelector('#logoff-button').addEventListener('click', function (e) {
//  e.preventDefault();
//  LogOff();
//});

// hometab event listener
document.querySelector('#home-tab-link').addEventListener('click', function (e) {
  e.preventDefault();
  showPanel('homeTab');
});

// account tab click event listener
document
  .querySelector('#account-tab-link')
  .addEventListener('click', function (e) {
    e.preventDefault();
    //showPanel('accountTab');
    if (userInfo == undefined) {
      document.querySelector('#accountSettingsTab').innerHTML = "";
      showPanel('accountTab');
    } else if (userInfo.loggedIn) {
      showPanel('accountSettingsTab');
    }

  });

// window load event listener
window.addEventListener('load', function (e) {
  console.log('app loaded');
  GetQuestions(); // this fetches all the quiz questions on load
});

// create account form submit event listener
document
  .querySelector('#create-account-form')
  .addEventListener('submit', function (e) {
    handleCreateAccountFormSubmit(e);
  });

// login form submit event listener
document.querySelector('#login-form').addEventListener('submit', function (e) {
  handleLoginFormSubmit(e);
});
