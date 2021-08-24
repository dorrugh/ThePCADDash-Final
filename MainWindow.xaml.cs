using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.IO;
using System.ComponentModel;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace ThePCADDash_Final
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //assigning variable to my  SQL database and table
        ThePCADDashEntities1 entities1 = new ThePCADDashEntities1();
        Scores1 entities = new Scores1();

        DispatcherTimer gameTimer = new DispatcherTimer();

        //Using rectangles to describe the hit box for collision detection 
        Rect playerhit;
        Rect groundhit;
        Rect obstaclehit;

        //boolean to check whether player is jumping or not
        bool jumping;

        int force = 20;
        int speed = 5;

        Random rnd = new Random();
        bool GameOver;

        // make a sprite int double variable, this will be used to swap the sprites for player
        double sprintInt = 0;

        ImageBrush playerSprite = new ImageBrush();
        ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush obstacleSprite = new ImageBrush();
        
        List<int> obstaclePosition = new List<int>() { 320, 310, 300, 305, 315 }; //determines the obstacle positons

        int score = 0;
        public MainWindow()
        {
            _ = TelemetryConfiguration.Active; //this fucntion is used to track the game's metric using "Application insights" on Azure portal.

            InitializeComponent();
            PCAD2.Focus();
            gameTimer.Tick += GameEngine;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            //fills the background with the passed image 
            backgroundSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/background.gif"));
            background.Fill = backgroundSprite;
            background2.Fill = backgroundSprite;
            StartGame();
        }
        private void GameEngine(object sender, EventArgs e)
        {
            _ = TelemetryConfiguration.Active; //this fucntion is used to track the game's metric using "Application insights" on Azure portal.
            Canvas.SetLeft(background, Canvas.GetLeft(background) - 3);
            Canvas.SetLeft(background2, Canvas.GetLeft(background2) - 3);

            if (Canvas.GetLeft(background) < -1262)
            {
                Canvas.SetLeft(background, Canvas.GetLeft(background2) + background2.Width);
            }
            if (Canvas.GetLeft(background2) < -1262)
            {
                Canvas.SetLeft(background2, Canvas.GetLeft(background) + background.Width);
            }

            Canvas.SetTop(player, Canvas.GetTop(player) + speed);
            Canvas.SetLeft(obstacle, Canvas.GetLeft(obstacle) - 12);

            Score.Content = "Score: " + score;

            playerhit = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width - 15, player.Height);
            obstaclehit = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.Width, obstacle.Height);
            groundhit = new Rect(Canvas.GetLeft(ground), Canvas.GetTop(ground), ground.Width, ground.Height);

            //If the player hit box intersects with the ground hit box. He will run on top of it
            if (playerhit.IntersectsWith(groundhit))
            {
                speed = 0;

                Canvas.SetTop(player, Canvas.GetTop(ground) - player.Height);

                jumping = false;

                sprintInt += .5;

                if (sprintInt > 8)
                {
                    sprintInt = 1;
                }

                RunSprite(sprintInt);
            }
            if (jumping ==true)
            {
                speed = -9;

                force-= 1;
            }
            else
            {
                speed = 25;
            }
            if (force < 0)
            {
                jumping = false;
            }
            if (Canvas.GetLeft(obstacle) < -50)
            {
                Canvas.SetLeft(obstacle, 950);

                Canvas.SetTop(obstacle, obstaclePosition[rnd.Next(0, obstaclePosition.Count)]);

                score += 1;
            }
            //if player hits the obstacle it will end the game
            if (playerhit.IntersectsWith(obstaclehit))
            {
                GameOver = true;
                gameTimer.Stop();
            }


            if (GameOver==true)
            {
                obstacle.Stroke = Brushes.Black;
                obstacle.StrokeThickness = 1;

                player.Stroke = Brushes.Red;
                player.StrokeThickness = 1;
               
                Score.Content = "Score: " + score + " Press Enter to play again!!";

                entities.Score = score;
                entities1.Scores1.Add(entities);
                entities1.SaveChanges();
                entities1 = null;
                entities1.Scores1.ToList();
            }
            else
            {

                player.StrokeThickness = 0;
                obstacle.StrokeThickness = 0;
            }

        }    
        //Allows the game to start over once enter key is pressed
        private void KeyisDown(object sender, KeyEventArgs e)
        {
            _ = TelemetryConfiguration.Active; //this fucntion is used to track the game's metrics scuh as CPU usage and Memory Consumption using "Application insights" in the Azure portal.
            if (e.Key == Key.Enter && GameOver==true)
            {
                StartGame();
            }
        } 

        //allows the player to jump to when the space key is up
        private void KeyisUp(object sender, KeyEventArgs e)
        {
            _ = TelemetryConfiguration.Active; //this fucntion is used to track the game's metric using "Application insights" on Azure portal.

             //If the space key is up it will initiate
            if (e.Key == Key.Space && jumping == false && Canvas.GetTop(player) > 260)
            {
                jumping = true;
                force = 15;
                speed = -12;

                //changing the player to look like he's/she's jumping
                playerSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/frame_04_delay-0.08s.gif"));
            }
        }
        private void StartGame()
        {
            _ = TelemetryConfiguration.Active; //this fucntion is used to track the game's metric using "Application insights" on Azure portal.
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262); //sets the game's 

            Canvas.SetLeft(player, 110);
            Canvas.SetTop(player, 140);

            Canvas.SetLeft(obstacle, 950);
            Canvas.SetTop(obstacle, 310);

            RunSprite(10);

            obstacleSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/obstacle.png")); //assigns the obstacle 
            obstacle.Fill = obstacleSprite;

            jumping = false;
            GameOver = false;
            score = 0;

            Score.Content = $"Score: {score} ";

            gameTimer.Start();
        }

        // this is the run sprite function, this function takes one argument inside its brackets
        // it takes a double variable called i
        // we will use this i to change the images for the player
        private void RunSprite(double i)
        {
            //this fucntion is used to track the game's metric using "Application insights" on Azure portal.
            _ = TelemetryConfiguration.Active;

            // below is the switch statement that will change the playersprite frame by frame to create the running animation
            // when the i value changes between 1 and 8 it will assign appropriate sprite to the player sprite

            List<string> images = new List<string>()
            {
                "pack://application:,,,/images/frame_00_delay-0.08s.gif",
                "pack://application:,,,/images/frame_01_delay-0.08s.gif",
                "pack://application:,,,/images/frame_02_delay-0.08s.gif",
                "pack://application:,,,/images/frame_03_delay-0.08s.gif",
                "pack://application:,,,/images/frame_04_delay-0.08s.gif",
                "pack://application:,,,/images/frame_05_delay-0.08s.gif",
                "pack://application:,,,/images/frame_06_delay-0.08s.gif",
                "pack://application:,,,/images/frame_07_delay-0.08s.gif",
                "pack://application:,,,/images/frame_08_delay-0.08s.gif",
                "pack://application:,,,/images/frame_09_delay-0.08s.gif",
                "pack://application:,,,/images/frame_10_delay-0.08s.gif",
                "pack://application:,,,/images/frame_11_delay-0.08s.gif"
            };

            playerSprite.ImageSource = new BitmapImage(new Uri(images[(int)i]));

            player.Fill = playerSprite;
        }
        //Enables a back button click event to go back to the UI interface if the user so pleases
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Users\wthom\source\repos\ThePCADDashUI\bin\Debug\ThePCADDashUI.exe");
            Close();

        }

        private void CloseGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MinimizeGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
