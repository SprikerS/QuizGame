namespace QuizGame
{
    internal class QuestionModel
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public int CorrectAnswer { get; set; }
    }
}
