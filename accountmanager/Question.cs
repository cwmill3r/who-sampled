using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace accountmanager
{
    public class Question
    {
        // just a container for info related to account
        public int questionId;
        public int creatorId;
        public string questionText;
        public string videoId;
        public string correctAnswerText;
        public List<WrongAnswer> wrongAnswers;
    }
}