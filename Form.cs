using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace QuizGame
{
    public partial class Form : System.Windows.Forms.Form
    {
        // Frase dinámica que puedes cambiar según desees
        private string phrase = "Cada pequeño paso que das hoy te acerca más a tus grandes sueños de mañana";

        // Lista de preguntas cargadas desde el archivo JSON
        ListQuestions listQuestions = new ListQuestions();

        // Nodo de pregunta actual
        private NodeQuestion currentQuestion;

        // Pila para las palabras obtenidas hasta ahora
        private Stack<string> obtainedWords = new Stack<string>();

        // Arreglo para las palabras obtenidas hasta ahora
        private string[] words;

        private int correctAnswers = 0;
        private int incorrectAnswers = 0;
        private int totalQuestions = 0;

        public Form()
        {
            InitializeComponent();
            InitializePhrase();
            LoadQuestions();
            UpdateObtainedPhrase();
        }

        private void LoadQuestions()
        {
            try
            {
                listQuestions.Clear(); // Limpiar la lista de preguntas (opcional)

                // Leer JSON desde los recursos y convertir byte[] a string
                byte[] jsonDataBytes = Properties.Resources.questions;
                string jsonData = Encoding.UTF8.GetString(jsonDataBytes);

                // Configurar opciones para ignorar mayúsculas/minúsculas en nombres de propiedades
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Deserializar JSON con opciones
                List<QuestionModel> allQuestions = JsonSerializer.Deserialize<List<QuestionModel>>(jsonData, options);

                // Mezclar preguntas y poner un limite si se desea
                List<QuestionModel> questions = allQuestions
                    .OrderBy(_ => Guid.NewGuid()) // Ordenar de forma aleatoria
                    //.Take(20) // Seleccionar las primeras 20 preguntas
                    .ToList();

                // Agregar preguntas a la lista de preguntas
                foreach (var question in questions)
                {
                    listQuestions.Add(question.Id, question.Question, question.Answers, question.CorrectAnswer);
                }

                // Configurar la pregunta actual como la primera en la lista
                currentQuestion = listQuestions.Get(0); // Obtener la primera pregunta
                SetQuestionData(currentQuestion);

                // Verificar selección de respuesta al inicio
                CheckAnswerSelected();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar las preguntas: {ex.Message}");
            }
        }

        // Inicializa la frase separando sus palabras en un array
        private void InitializePhrase()
        {
            words = phrase.Split(' '); // Separar la frase en palabras y almacenarlas en el arreglo
            obtainedWords.Clear();  // Vacía la pila al inicio
        }

        private void SetQuestionData(NodeQuestion question)
        {
            lblQuestion.Text = question.Question;
            rbAnswer1.Text = question.Answers[0];
            rbAnswer2.Text = question.Answers[1];
            rbAnswer3.Text = question.Answers[2];
            rbAnswer4.Text = question.Answers[3];

            // Deseleccionar todos los RadioButtons para la nueva pregunta
            foreach (RadioButton rb in gbAnswers.Controls.OfType<RadioButton>())
            {
                rb.Checked = false;
            }

            // Verificar selección de respuesta para activar/desactivar `btnNext`
            CheckAnswerSelected();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Verificar respuesta y avanzar solo si el juego debe continuar
            int selectedAnswer = GetSelectedAnswerIndex(); // Obtener la respuesta seleccionada
            bool shouldContinue = VerifyAnswer(selectedAnswer);

            if (shouldContinue) // Continuar solo si el juego debe continuar
            {
                if (currentQuestion.After != null) // Verifica si existe una siguiente pregunta
                {
                    currentQuestion = currentQuestion.After;
                    SetQuestionData(currentQuestion);
                }
                else
                {
                    // Usuario llegó al final de las preguntas y no completó la frase
                    EndGame(TypeEndGame.Restart);
                }
            }
        }

        // Método para verificar la respuesta y actualizar la frase obtenida
        private bool VerifyAnswer(int selectedAnswer)
        {
            // Realiza el Push o Pop primero
            if (selectedAnswer == currentQuestion.CorrectAnswer)
            {
                // Respuesta correcta: agrega la siguiente palabra si existe
                int nextWordIndex = obtainedWords.Count;
                if (nextWordIndex < words.Length)
                {
                    obtainedWords.Push(words[nextWordIndex]);
                    correctAnswers++;
                }
            }
            else
            {
                // Respuesta incorrecta: elimina la última palabra si existe
                if (obtainedWords.Count > 0)
                {
                    obtainedWords.Pop();
                    incorrectAnswers++;
                }
            }

            // Verifica si el usuario ha ganado o perdido
            if (obtainedWords.Count == words.Length || obtainedWords.Count == 0)
            {
                TypeEndGame message = obtainedWords.Count == words.Length
                    ? TypeEndGame.Success
                    : TypeEndGame.Failure;
                EndGame(message);

                return false; // Termina el juego si ganó o perdió
            }

            // Actualiza la frase mostrada en pantalla
            UpdateObtainedPhrase();
            return true; // Indica que el juego debe continuar
        }

        // Enumeración para los tipos de fin de juego
        private enum TypeEndGame
        {
            Success,
            Failure,
            Restart
        }

        // Muestra un mensaje de fin de juego con la opción de reiniciar o salir
        private void EndGame(TypeEndGame type)
        {
            MessageBoxIcon messageBoxIcon = MessageBoxIcon.None;
            string message = string.Empty;
            string title = string.Empty;

            switch (type)
            {
                case TypeEndGame.Success:
                    messageBoxIcon = MessageBoxIcon.Information;
                    title = "Felicidades has ganado";
                    message = $"La frase de tu galletas de fortuna es:\n\n{ string.Join(" ", obtainedWords.Reverse()) }";
                    break;
                case TypeEndGame.Failure:
                    messageBoxIcon = MessageBoxIcon.Error;
                    title = "Lo siento has perdido";
                    message = "No has logrado completar la frase en los intentos permitidos";
                    break;
                case TypeEndGame.Restart:
                    messageBoxIcon = MessageBoxIcon.Warning;
                    title = "Juego terminado se te acabaron las opciones";
                    message = "Llegaste al final de las preguntas y no completaste la frase";
                    break;
            }

            var result = MessageBox.Show($"{message}\n\n¿Quieres reiniciar?", title, MessageBoxButtons.YesNo, messageBoxIcon);
            if (result == DialogResult.Yes)
            {
                RestartGame();
            }
            else
            {
                Application.Exit();
            }
        }

        // Reinicia el juego desde la primera pregunta y limpia la pila
        private void RestartGame()
        {
            obtainedWords.Clear();  // Vacía la pila
            InitializePhrase();     // Reinicia la frase y pila
            LoadQuestions();        // Carga las preguntas nuevamente

            correctAnswers = 0;     // Reinicia las respuestas correctas
            incorrectAnswers = 0;   // Reinicia las respuestas incorrectas
            UpdateObtainedPhrase(); // Limpia la frase obtenida en la UI
        }

        // Actualiza la frase obtenida hasta ahora en algún elemento de la UI
        private void UpdateObtainedPhrase()
        {
            lblCorrect.Text = correctAnswers.ToString("D2");
            lblIncorrect.Text = incorrectAnswers.ToString("D2");

            // Actualizar el número de preguntas respondidas
            int answeredQuestions = listQuestions.Count - (correctAnswers + incorrectAnswers);
            lblNumberQuestions.Text = answeredQuestions.ToString("D2");

            // Actualizar el número de preguntas faltantes
            int missingQuestions = words.Length - obtainedWords.Count;
            lblMissingQuestions.Text = missingQuestions.ToString("D2");

            // Actualizar el número de corazones obtenidos
            lblHearts.Text = obtainedWords.Count.ToString("D2");
        }

        private int GetSelectedAnswerIndex()
        {
            if (rbAnswer1.Checked) return 0;
            if (rbAnswer2.Checked) return 1;
            if (rbAnswer3.Checked) return 2;
            if (rbAnswer4.Checked) return 3;
            return -1; // Ninguna seleccionada
        }

        private void CheckAnswerSelected()
        {
            btnNext.Enabled = gbAnswers.Controls.OfType<RadioButton>().Any(rb => rb.Checked);
        }

        private void rbAnswer_CheckedChanged(object sender, EventArgs e)
        {
            CheckAnswerSelected();
        }
    }
}
