//const sampleQuiz = {
//  quizId: 0,
//  questions: [
//    {
//      questionId: 0,
//      creatorId: 7,
//      questionText: 'who sampled?',
//      videoId: 'cFU-FJzPE80',
//      correctAnswerText: 'Hotline Bling by Drake',
//      wrongAnswers: [
//        'Work Out by J Cole',
//        'California Love by Tupac',
//        "Gangsta's Paradise by Coolio"
//      ]
//    },
//    {
//      questionId: 1,
//      creatorId: 7,
//      videoId: 'rMB-wx7LmdQ',
//      questionText: 'who sampled?',
//      correctAnswerText: 'Mask Off by Future',
//      wrongAnsAnwers: [
//        'Work Out by J Cole',
//        'California Love by Tupac',
//        "Gangsta's Paradise by Coolio"
//      ]
//    },
//    {
//      questionId: 2,
//      creatorId: 7,
//      videoId: 'xKISdd2mKzU',
//      questionText: 'who sampled?',
//      correctAnswerText: 'My Name Is by Eminem',
//      wrongAnswers: [
//        'Work Out by J Cole',
//        'California Love by Tupac',
//        "Gangsta's Paradise by Coolio"
//      ]
//    },
//    {
//      questionId: 3,
//      creatorId: 7,
//      videoId: 'zY0--b6DLqQ',
//      questionText: 'who sampled?',
//      correctAnswerText: 'Regulate by Warren G',
//      wrongAnswers: [
//        'Work Out by J Cole',
//        'California Love by Tupac',
//        "Gangsta's Paradise by Coolio"
//      ]
//    },
//    {
//      questionId: 4,
//      creatorId: 7,
//      videoId: 'Mrd14PxaUco',
//      questionText: 'who sampled?',
//      correctAnswerText: 'Gold Digger by Kanye West',
//      wrongAnswers: [
//        'Work Out by J Cole',
//        'California Love by Tupac',
//        "Gangsta's Paradise by Coolio"
//      ]
//    }
//  ]
//};
// example function that maps over the json and writes to the console

// declare the global quiz questions which are fetched on window load
let quizQuestions = undefined;

function writeSampleQuizToConsole() {
  sampleQuiz.questions.map(function(question) {
    console.log(question.questionText);
    console.log(question.correctAns);
    question.wrongAnsArr.map(function(answ) {
      console.log(answ);
    });
  });
}

// content panels used in hiding showing
const contentPanels = [
  'homeTab',
  'aboutTab',
  'accountTab',
  'addQuestionTab',
  'accountSettingsTab',
  'createAccountTab'
];

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

// login form submit handler
function handleLoginFormSubmit(e) {
  e.preventDefault();
  const id = document.querySelector('#logonId').value;
  console.log(id);
  const pass = document.querySelector('#logonPassword').value;
  console.log(pass);
  LogOn(id, pass);
  return false;
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

// create account event listener
document
  .querySelector('#create-account-link')
  .addEventListener('click', function(e) {
    e.preventDefault();
    showPanel('createAccountTab');
  });

// logoff button event listener
document.querySelector('#logoff-button').addEventListener('click', function(e) {
  e.preventDefault();
  LogOff();
});

// hometab event listener
document.querySelector('#home-tab-link').addEventListener('click', function(e) {
  e.preventDefault();
  showPanel('homeTab');
});

// account link click event listener
document
  .querySelector('#account-tab-link')
  .addEventListener('click', function(e) {
    e.preventDefault();
    showPanel('accountTab');
  });

// window load event listener
window.addEventListener('load', function(e) {
  console.log('app loaded');
  GetQuestions(); // this fetches all the quiz questions on load
});

// create account form submit event listener
document
  .querySelector('#create-account-form')
  .addEventListener('submit', function(e) {
    handleCreateAccountFormSubmit(e);
  });

// login form submit event listener
document.querySelector('#login-form').addEventListener('submit', function(e) {
  handleLoginFormSubmit(e);
});

// BELOW ARE API UTILITIY TYPE THINGS

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
//passes account info to the server, to create an account request
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
      alert('Account request pending approval...');
    },
    error: function(e) {
      alert('boo...');
    }
  });
}

//HERE'S AN EXAMPLE OF AN AJAX CALL WITH JQUERY!
function LogOn(userId, pass) {
  //the url of the webservice we will be talking to
  var webMethod = 'http://localhost:50406/AccountServices.asmx/LogOn';
  //the parameters we will pass the service (in json format because curly braces)
  //note we encode the values for transmission over the web.  All the \'s are just
  //because we want to wrap our keynames and values in double quotes so we have to
  //escape the double quotes (because the overall string we're creating is in double quotes!)
  var parameters =
    '{"uid":"' + encodeURI(userId) + '","pass":"' + encodeURI(pass) + '"}';

  //jQuery ajax method
  $.ajax({
    //post is more secure than get, and allows
    //us to send big data if we want.  really just
    //depends on the way the service you're talking to is set up, though
    type: 'POST',
    //the url is set to the string we created above
    url: webMethod,
    //same with the data
    data: parameters,
    //these next two key/value pairs say we intend to talk in JSON format
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    //jQuery sends the data and asynchronously waits for a response.  when it
    //gets a response, it calls the function mapped to the success key here
    success: function(msg) {
      //the server response is in the msg object passed in to the function here
      //since our logon web method simply returns a true/false, that value is mapped
      //to a generic property of the server response called d (I assume short for data
      //but honestly I don't know...)
      if (msg.d) {
        console.log(msg.d);
        alert('logon success - remove for production');
        showPanel('accountSettingsTab');
        quizQuestions = msg.d;
      } else {
        //server replied false, so let the user know
        //the logon failed
        alert('logon failed');
      }
    },
    error: function(e) {
      //if something goes wrong in the mechanics of delivering the
      //message to the server or the server processing that message,
      //then this function mapped to the error key is executed rather
      //than the one mapped to the success key.  This is just a garbage
      //alert becaue I'm lazy
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
        //we logged off, so go back to logon page,
        //stop checking messages
        //and clear the chat panel
        alert('sucessful logoff');
        showPanel('accountTab');
        //HideMenu();
      } else {
        alert('something went wrong with log off');
      }
    },
    error: function(e) {
      alert('boo...');
    }
  });
}
