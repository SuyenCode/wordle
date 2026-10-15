using Microsoft.VisualBasic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WORDLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> words = new List<string> { "START", "EVENT", "SPOOK", "PURSE", "SONGS", "MUSIC", "VIDEO", "PARTY", "TRAIN", "GIANT", "DWARF", "QUEEN", "PIANO", "KNIGHT", "JELLY", "MOUSE", "TIGER", "CLOUD", "STORE", "CHAIR" };
        private List<string> letter = new List<string> { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        private Label[] labels;
        private Button[] letterButtons;

        private Label[][] allLabels;
        private int currentAttempt = 0;

        Dictionary<char, int> guessedLetters = new Dictionary<char, int>();
        Dictionary<char, int> amountOfSameLetters = new Dictionary<char, int>();


        private Random rnd = new Random();
        private string word;

        DateTime now = DateTime.Now;
        DispatcherTimer timer = new DispatcherTimer();

        private int debugCounter = 0;
        private int counterGreen = 4;
        private int counterYellow = 4;
        private int counterGrey = 0;

        private string green = "";
        private string yellow = "";
        private string grey = "";

        string[] greenLetters = new string[5];
        string[] yellowLetters = new string[5];
        string[] greyLetters = new string[26];
        private int correctWordsCounter = 0;

        private int amountGems = 0;
        private int wonGame = 0;
        private List<string> greenLettersList = new List<string>();
        private List<string> yellowLettersList = new List<string>();
        private List<string> greyLettersList = new List<string>();
        private Dictionary<string, bool> greenGemDictionary = new Dictionary<string, bool>();
        private int amountFalse = 0;
        private Dictionary<string, bool> yellowGemDictionary = new Dictionary<string, bool>();
        private int amountYellowFalse = 0;
        private int totalscore = 100;


        private SolidColorBrush ButtonDefaultColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#656780");
        private SolidColorBrush BackgroundLabelColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#191A24");
        private SolidColorBrush BorderLabelColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#3D4054");
        private SolidColorBrush GuessedLabelColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#656780");
        private SolidColorBrush GuessedButtonColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#3D4054");
        private SolidColorBrush GreenColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#79B851");
        private SolidColorBrush OrangeColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#F3C237");
        private SolidColorBrush HoverButtonColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#898BA4");
        private SolidColorBrush HoverButtonOrangeColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFD700");
        private SolidColorBrush HoverButtonGreenColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#90EE90");

        public MainWindow()
        {
            InitializeComponent();
            Keyboard.ClearFocus();
            letterButtons = new Button[]{
                aButton, bButton, cButton, dButton, eButton, fButton, gButton,
                hButton, iButton, jButton, kButton, lButton, mButton, nButton,
                oButton, pButton, qButton, rButton, sButton, tButton, uButton,
                vButton, wButton, xButton, yButton, zButton
            };

            allLabels = new Label[][]
            {
                new Label[] { label1, label2, label3, label4, label5 },
                new Label[] { label6, label7, label8, label9, label10 },
                new Label[] { label11, label12, label13, label14, label15 },
                new Label[] { label16, label17, label18, label19, label20 },
                new Label[] { label21, label22, label23, label24, label25 },
                new Label[] { label26, label27, label28, label29, label30 },
            };
            labels = allLabels[currentAttempt];

            debugTextBlock.Visibility = Visibility.Hidden;

            Window_Restart();

            timer = new DispatcherTimer();

            timer.Tick += gameTimer;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            gemPurchased();
            Filling_Dictionary();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key >= Key.A && e.Key <= Key.Z)
            {
                if (e.Key != Key.Back && e.Key != Key.Delete)
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        if (string.IsNullOrEmpty(labels[i].Content.ToString()))
                        {
                            labels[i].Content = e.Key.ToString();
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = labels.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(labels[i].Content.ToString()))
                        {
                            labels[i].Content = string.Empty;
                            break;
                        }
                    }

                }

            }
            if (e.Key == Key.Enter && AllLabelsFilled())
            {
                Border_Color();
                Key_Border_Color();


                if (currentAttempt < allLabels.Length - 1)
                {
                    currentAttempt++;
                    labels = allLabels[currentAttempt];
                    ResetLabels();
                }
                else
                {
                    MessageBox.Show($"You used all your attempts. The word was {word}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }

            if (e.Key == Key.F2)
            {
                ToggleDebugVisibility();

            }
        }

        private bool AllLabelsFilled()
        {
            foreach (var lbl in labels)
            {
                if (string.IsNullOrEmpty(lbl.Content.ToString()))
                    return false;
            }
            return true;
        }

        private void Border_Color()
        {
            {
                guessedLetters.Clear();
                amountOfSameLetters.Clear();
                wonGame = 0;


                foreach (char c in word)
                {
                    if (amountOfSameLetters.ContainsKey(c))
                        amountOfSameLetters[c]++;
                    else
                        amountOfSameLetters[c] = 1;
                }

                for (int i = 0; i < labels.Length; i++)
                {
                    char letter = char.Parse(labels[i].Content.ToString());


                    if (word[i] == letter)
                    {
                        wonGame++;
                        labels[i].Background = GreenColor;
                        labels[i].BorderBrush = GreenColor;

                        if (guessedLetters.ContainsKey(letter))
                        {
                            guessedLetters[letter]++;
                        }
                        else
                        {
                            guessedLetters[letter] = 1;
                        }

                        if (!greenLetters.Contains(letter.ToString()))
                        {
                            AddLetterToGreens(letter.ToString());

                            if (yellowLetters.Contains(letter.ToString()))
                            {
                                RemoveYellowLetters(letter.ToString());
                            }
                        }

                    }
                }

                for (int i = 0; i < labels.Length; i++)
                {
                    char letter = char.Parse(labels[i].Content.ToString());

                    if (labels[i].Background == BackgroundLabelColor && word.Contains(letter))
                    {
                        if (guessedLetters.ContainsKey(letter) && guessedLetters[letter] < amountOfSameLetters[letter])
                        {
                            labels[i].Background = OrangeColor;
                            labels[i].BorderBrush = OrangeColor;
                            guessedLetters[letter]++;

                            if (!yellowLetters.Contains(letter.ToString()) && !greenLetters.Contains(letter.ToString()))
                            {
                                AddLetterToYellows(letter.ToString());
                            }
                        }
                        else if (!guessedLetters.ContainsKey(letter))
                        {
                            labels[i].Background = OrangeColor;
                            labels[i].BorderBrush = OrangeColor;
                            guessedLetters[letter] = 1;

                            if (!yellowLetters.Contains(letter.ToString()) && !greenLetters.Contains(letter.ToString()))
                            {
                                AddLetterToYellows(letter.ToString());
                            }
                        }
                        else
                        {
                            labels[i].Background = GuessedLabelColor;
                            labels[i].BorderBrush = GuessedLabelColor;

                            if (!greyLetters.Contains(letter.ToString()))
                            {
                                AddLetterToGreys(letter.ToString());
                            }
                        }
                    }
                    else if (!word.Contains(letter))
                    {
                        labels[i].Background = GuessedLabelColor;
                        labels[i].BorderBrush = GuessedLabelColor;

                        if (!greyLetters.Contains(letter.ToString()))
                        {
                            AddLetterToGreys(letter.ToString());
                        }
                    }
                }
                if (wonGame == 5)
                {
                    Score_Update(100);
                    gameStatusLabel.Content = "You found the word!";
                    MessageBox.Show("You found the word!", "Congratulations!", MessageBoxButton.OK, MessageBoxImage.Information);
                    correctWordsCounter++;
                    timer.Stop();
                    statusBarTextBlock.Text = $"aantal juiste woorden: {correctWordsCounter}";


                }
                else
                {
                    Score_Update(-10);
                }
            }
        }

        private void RemoveYellowLetters(string letterToRemove)
        {
            counterYellow = yellowLetters.Length;
            if (yellowLetters.Contains(letterToRemove))
            {
                string[] newYellowLetters = new string[yellowLetters.Length - 1];
                int j = 0;
                for (int i = 0; i < yellowLetters.Length; i++)
                {
                    if (yellowLetters[i] != letterToRemove && yellowLetters[i] != null)
                    {
                        newYellowLetters[j++] = yellowLetters[i];
                        counterYellow--;
                    }
                }
                yellowLetters = newYellowLetters;
                Array.Sort(yellowLetters);
            }
            yellow = "";
            foreach (string yellowLetter in yellowLetters)
            {
                if (yellowLetter != null)
                {
                    yellow += " " + yellowLetter;
                }

            }

        }
        private void ResetLabels()
        {
            foreach (var lbl in labels)
            {
                lbl.Content = string.Empty;
                lbl.Background = BackgroundLabelColor;
            }
        }

        private void Window_Restart()
        {
            ResetLabels();

            int index = rnd.Next(0, words.Count);
            word = words[index];
        }

        private void key_Click(object sender, RoutedEventArgs e)
        {
            Button letterButton = (Button)sender;
            if (letterButton.Content.ToString() == "Enter")
            {
                if (AllLabelsFilled())
                {
                    Border_Color();
                    Key_Border_Color();
                    if (currentAttempt < allLabels.Length - 1 || wonGame == 5)
                    {
                        currentAttempt++;
                        labels = allLabels[currentAttempt];
                        ResetLabels();
                    }
                    else
                    {
                        gameStatusLabel.Content = "Game Over";
                        MessageBox.Show($"You used all your attempts. The word was {word}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                if (letterButtons.Contains(letterButton))
                {
                    for (int i = 0; i < labels.Length; i++)
                    {
                        if (labels[i].Content.ToString().Length == 0)
                        {
                            labels[i].Content = letterButton.Content.ToString();
                            break;
                        }
                    }
                }
            }
            enterButton.Focus();
        }
        private void Key_Border_Color()
        {
            char letter;
            string letterString;
            int index;

            for (int i = 0; i < letterButtons.Length; i++)
            {
                for (int j = 0; j < labels.Length; j++)
                {
                    letter = char.Parse(labels[j].Content.ToString());
                    index = Array.FindIndex(letterButtons, btn => btn.Name == $"{letter.ToString().ToLower()}Button");
                    letterString = letter.ToString();
                    if (word.Contains(letter))
                    {

                        if (word[j] == letter)
                        {
                            letterButtons[index].Background = GreenColor;
                            if (yellowGemDictionary[letter.ToString()] == true)
                            {
                                amountFalse++;
                                amountYellowFalse++;
                            }
                            greenGemDictionary[letter.ToString()] = false;
                            yellowGemDictionary[letter.ToString()] = false;
                        }
                        else
                        {
                            if (letterButtons[index].Background != GreenColor)
                            {
                                letterButtons[index].Background = OrangeColor;
                                if (yellowGemDictionary[letter.ToString()] == true)
                                {
                                    amountYellowFalse++;
                                }
                                yellowGemDictionary[letter.ToString()] = false;

                            }
                        }
                    }
                    else
                    {
                        letterButtons[index].Background = GuessedButtonColor;
                    }
                }
            }
        }
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = labels.Length - 1; i >= 0; i--)
            {
                if (labels[i].Content.ToString().Length == 1)
                {
                    labels[i].Content = string.Empty;
                    break;
                }
            }

        }
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            gameStatusLabel.Content = string.Empty;

            MessageBox.Show("New Game started!", "New Game", MessageBoxButton.OK);

            ResetLabels();

            wonGame = 0;

            gameStatusLabel.Content = "";

            foreach (var button in letterButtons)
            {
                button.Background = ButtonDefaultColor;
            }

            foreach (Label[] row in allLabels)
            {
                foreach (Label label in row)
                {
                    label.Content = "";
                    label.Background = BackgroundLabelColor;
                    label.BorderBrush = BorderLabelColor;
                }
            }

            currentAttempt = 0;
            labels = allLabels[currentAttempt];

            int index = rnd.Next(0, words.Count);
            word = words[index];

            guessedLetters.Clear();
            amountOfSameLetters.Clear();


            Array.Clear(greenLetters, 0, greenLetters.Length);
            Array.Clear(yellowLetters, 0, yellowLetters.Length);
            Array.Clear(greyLetters, 0, greyLetters.Length);

            Array.Resize(ref yellowLetters, 5);

            green = "";
            yellow = "";
            grey = "";

            counterGreen = 4;
            counterYellow = 0;
            counterGrey = 0;

            timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += gameTimer;
            timer.Start();

            now = DateTime.Now;

            Filling_Dictionary();
            counterGreen = 4;
            counterYellow = 0;

            gemPurchased();

            amountFalse = 0;
            amountYellowFalse = 0;

        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Background == ButtonDefaultColor)
            {
                button.Background = HoverButtonColor;
            }
            else if (button.Background == OrangeColor)
            {
                button.Background = HoverButtonOrangeColor;
                button.ToolTip = "correct letter, incorrect place";
            }
            else if (button.Background == GreenColor)
            {
                button.Background = HoverButtonGreenColor;
                button.ToolTip = "correct letter, correct place";
            }
            else if (button.Background == GuessedButtonColor)
            {
                button.ToolTip = "incorrect letter";
            }
        }
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Background == HoverButtonColor)
            {
                button.Background = ButtonDefaultColor;
            }
            else if (button.Background == HoverButtonOrangeColor)
            {
                button.Background = OrangeColor;
            }
            else if (button.Background == HoverButtonGreenColor)
            {
                button.Background = GreenColor;
            }
        }

        /// <summary>
        /// Er word een variabele DispatcherTimer timer aangemaakt die het object van gametimer
        /// uitvoert. De interval is 1 seconden en wordt gestart.
        /// Ook debugTextBlock zichtbaar gezet of omgekeerd.
        /// </summary>
        private void ToggleDebugVisibility()
        {
            if (debugTextBlock != null)
            {
                if (debugTextBlock.Visibility == Visibility.Hidden)
                {
                    debugTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    debugTextBlock.Visibility = Visibility.Hidden;
                }
            }
        }
        private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (debugTextBlock != null)
            {
                if (debugTextBlock.Visibility == Visibility.Hidden)
                {
                    debugTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    debugTextBlock.Visibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// De content van de debugTextBlock wordt aangepast met de nieuwe TimeSpan difference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameTimer(object sender, EventArgs e)
        {
            TimeSpan difference = now - DateTime.Now;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("debug (F2)");
            sb.AppendLine($"woord timer: {difference:hh\\:mm\\:ss}");
            sb.AppendLine($"aantal pogingen: {currentAttempt}");
            sb.AppendLine($"correct woord: {word}");
            sb.AppendLine();

            sb.AppendLine("Green Letters: " + green +
                        "\nYellow Letters: " + yellow +
                        "\nGrey Letters: " + grey);



            debugTextBlock.Text = sb.ToString();
        }

        public void AddLetterToGreens(string letter)
        {
            int index;
            string key;
            List<string> greenLettersIndex = new List<string>();
            if (word.Contains(letter) && !greenLetters.Contains(letter))
            {

                greenLetters[counterGreen] = letter;
                green = "";
                Array.Sort(greenLetters);
                foreach (string greenLetter in greenLetters)
                {
                    index = 1;
                    foreach (Label lbl in labels)
                    {
                        if (lbl.Content.ToString() == greenLetter && lbl.Background == GreenColor)
                        {
                            green += " " + greenLetter + " " + "(" + index.ToString() + ")";

                            break;
                        }
                        index++;

                    }

                }
                counterGreen--;

            }
        }
        public void AddLetterToGreens2(string letter)
        {
            bool buttonGreen = false;
            foreach (Button button in letterButtons)
            {
                if (button.Content.ToString() == letter)
                {
                    buttonGreen = true;
                    break;
                }
            }
            if (word.Contains(letter) && !greenLetters.Contains(letter) && counterGreen >= 0)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    if (word[i].ToString() == letter && (labels[i].Content.ToString() == letter || buttonGreen == true))
                    {
                        greenLetters[i] = letter;

                        if (!green.Contains($"{letter}({i + 1})"))
                        {
                            green += $"{letter}({i + 1}) ";
                        }

                        counterGreen--;
                    }
                }
            }
        }

        public void AddLetterToYellows(string letter)
        {
            if (counterYellow >= 0)
            {
                yellowLetters[counterYellow] = letter;
                yellow = "";
                Array.Sort(yellowLetters);
                foreach (string yellowLetter in yellowLetters)
                {
                    if (yellowLetter != null)
                    {
                        yellow += " " + yellowLetter;
                    }

                }
                counterYellow--;
            }
        }

        public void AddLetterToGreys(string letter)
        {
            if (!word.Contains(letter) && !greyLetters.Contains(letter))
            {
                greyLetters[counterGrey] = letter;
                grey = "";
                Array.Sort(greyLetters);
                foreach (string greyLetter in greyLetters)
                {
                    if (greyLetter != null)
                    {
                        grey += " " + greyLetter;
                    }
                }
                counterGrey++;
            }
        }

        private void buyGemsButton_Click(object sender, RoutedEventArgs e)
        {
            int gems;

            string input = Interaction.InputBox("How many gems do you wish to buy? (1 euro = 100 gems). Enter the amount of euros you wish to spend.", "Buy Gems!", "10");
            if (int.TryParse(input, out int euro))
            {
                gems = euro * 100;
                amountGems += gems;
                MessageBox.Show($"You just became a wealthy WORLDER and bought {gems} GEMS!");
                gemsAmountLabel.Content = amountGems;

            }
            else
            {
                MessageBox.Show("Something went wrong. Try Entering a valid amount of euros to spend.", "Oops", MessageBoxButton.OK);
            }
            gemPurchased();
            enterButton.Focus();
        }

        private void Filling_Dictionary()
        {
            string letter;

            for (int i = 0; i < word.Length; i++)
            {
                letter = word[i].ToString();
                greenGemDictionary[letter] = true;
            }
            yellowGemDictionary = greenGemDictionary.ToDictionary(entry => entry.Key, entry => entry.Value); ;
        }

        private void gemPurchased()
        {
            if (amountGems >= 10 && wonGame != 5)
            {
                if (wonGame != 5)
                {
                    buyGreenButton.IsEnabled = true;
                    buyYellowButton.IsEnabled = true;
                    buyGreenButton.Background = ButtonDefaultColor;
                    buyYellowButton.Background = ButtonDefaultColor;
                }
                else
                {
                    buyGreenButton.IsEnabled = false;
                    buyYellowButton.IsEnabled = false;
                    buyGreenButton.Background = GuessedButtonColor;
                    buyYellowButton.Background = GuessedButtonColor;
                }
            }
            else
            {
                buyGreenButton.IsEnabled = false;
                buyYellowButton.IsEnabled = false;
                buyGreenButton.Background = GuessedButtonColor;
                buyYellowButton.Background = GuessedButtonColor;
            }
            enterButton.Focus();
        }
        private void buyGreenButton_Click(object sender, RoutedEventArgs e)
        {
            int randomIndex;
            bool value = false;

            if (amountFalse <= greenGemDictionary.Count)
            {
                do
                {
                    randomIndex = rnd.Next(0, greenGemDictionary.Count);
                    value = greenGemDictionary.Values.ElementAt(randomIndex);
                    if (amountFalse == greenGemDictionary.Count())
                    {
                        break;
                    }
                } while (!value);

                if (value)
                {
                    amountFalse++;
                    amountYellowFalse++;
                    counterYellow = yellowLetters.Length;

                    string key = greenGemDictionary.Keys.ElementAt(randomIndex);

                    AddLetterToGreens2(key);
                    RemoveYellowLetters(key);

                    foreach (Button button in letterButtons)
                    {
                        if (button.Content.ToString() == key)
                        {
                            if (button.Background == OrangeColor)
                            {
                                amountYellowFalse--;
                            }
                            button.Background = GreenColor;
                            greenGemDictionary[key] = false;
                            yellowGemDictionary[key] = false;
                            amountGems -= 10;
                            gemsAmountLabel.Content = amountGems.ToString();
                            break;
                        }
                    }
                }
            }


            if (amountFalse == greenGemDictionary.Count() || amountGems < 10)
            {
                buyGreenButton.IsEnabled = false;
                buyYellowButton.IsEnabled = false;
                buyGreenButton.Background = GuessedButtonColor;
                buyYellowButton.Background = GuessedButtonColor;
            }
            enterButton.Focus();
        }
        private void buyYellowButton_Click(object sender, RoutedEventArgs e)
        {
            int randomIndex;
            bool value = false;

            if (amountYellowFalse <= yellowGemDictionary.Count)
            {
                do
                {
                    randomIndex = rnd.Next(0, yellowGemDictionary.Count);
                    value = yellowGemDictionary.Values.ElementAt(randomIndex);

                } while (value == false);

                if (value == true)
                {
                    amountYellowFalse++;
                    counterYellow = yellowLetters.Length - 1;
                    string key = yellowGemDictionary.Keys.ElementAt(randomIndex);

                    AddLetterToYellows(key);

                    foreach (Button button in letterButtons)
                    {
                        if (button.Content.ToString() == key)
                        {
                            button.Background = OrangeColor;
                            yellowGemDictionary[key] = false;
                            amountGems -= 5;
                            gemsAmountLabel.Content = amountGems.ToString();
                            break;
                        }
                    }
                }
            }

            if (amountYellowFalse == yellowGemDictionary.Count || amountGems < 5)
            {
                buyYellowButton.IsEnabled = false;
                buyYellowButton.Background = GuessedButtonColor;
            }
            enterButton.Focus();
        }
        private void Score_Update(int score)
        {
            totalscore = totalscore + score;
            scoreBarTextBlock.Text = $"Total score: {totalscore.ToString()}";
        }
    }
}

