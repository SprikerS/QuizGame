using System;

namespace QuizGame
{
    internal class ListQuestions
    {
        private NodeQuestion head;
        private NodeQuestion tail;
        private int count;

        public ListQuestions()
        {
            head = null;
            tail = null;
            count = 0;
        }

        public void Add(int id, string question, string[] answers, int correctAnswer)
        {
            NodeQuestion newNode = new NodeQuestion(id, question, answers, correctAnswer);

            if (head == null)
            {
                head = newNode;
                tail = newNode;
            }
            else
            {
                tail.After = newNode;
                newNode.Before = tail;
                tail = newNode;
            }

            count++;
        }

        public NodeQuestion Get(int index)
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException("Index out of range");
            }

            NodeQuestion current = head;
            for (int i = 0; i < index; i++)
            {
                current = current.After;
            }

            return current;
        }

        public int Count
        {
            get { return count; }
        }

        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }
    }
}
