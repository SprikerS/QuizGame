namespace QuizGame
{
    internal class NodeQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }
        public NodeQuestion Before { get; set; }
        public NodeQuestion After { get; set; }

        public NodeQuestion(int id, string question, string[] answers, int correctAnswer)
        {
            Id = id;
            Question = question;
            Answers = answers;
            CorrectAnswer = correctAnswer;
        }
    }
}
