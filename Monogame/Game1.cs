﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Monogame
{
    public class Game1 : Game
    {
        int playerNumber = 0;

        float timeBetweenStreamUpdates = 0.01f;
        float timeSinceLastStreamUpdate = 0f;

        Texture2D player1Texture;
        Vector2 player1Position;

        Texture2D player2Texture;
        Vector2 player2Position;

        float acceleration;
        float frictionMultiplier;
        float slowdownFrictionMultiplier;
        Vector2 currentVelocity;
        float wallSlowdownMultiplier;
        
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        TcpClient tcpClient = new TcpClient();
        NetworkStream netStream;
        ASCIIEncoding asen = new ASCIIEncoding();

        public Game1(string IPAddress, int port, int loginSelection)
        {
            Console.SetWindowSize(75, 15);

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            try
            {
                Console.Clear();
                Console.WriteLine("Attempting to connect to game server...");
                tcpClient.Connect(IPAddress, port);
                netStream = tcpClient.GetStream();
                netStream.Write(asen.GetBytes(loginSelection.ToString()));

                Console.WriteLine();
                Console.WriteLine("Connected.");
                Console.WriteLine();

                Task.Run(() => StreamReader());
            }
            catch
            {
                Console.WriteLine("Failed to connect to game server.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

        }

        protected override void Initialize()
        {
            player1Position = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            acceleration = 16000f;
            wallSlowdownMultiplier = 0.7f;
            frictionMultiplier = 0.75f;
            slowdownFrictionMultiplier = 0.86f;

            player2Position = new Vector2(_graphics.PreferredBackBufferWidth / 2 + 200, _graphics.PreferredBackBufferHeight / 2);

            base.Initialize();
            Console.WriteLine("Monogame Initialized.");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            player1Texture = Content.Load<Texture2D>("player1");
            player2Texture = Content.Load<Texture2D>("player2");
            Console.WriteLine("Textures Loaded.");
        }

        private void StreamReader()
        {
            Console.WriteLine("Started network stream reader.");

            byte[] selectionBuffer = new byte[8];
            int selectionBufferCount = netStream.Read(selectionBuffer);

            for (int i = 0; i < selectionBufferCount; i++)
            {
                char connectedPlayer = Convert.ToChar(selectionBuffer[i]);
                Console.WriteLine("Connected as player " + connectedPlayer + ".");

                if (connectedPlayer == '1')
                {
                    playerNumber = 1;
                    Console.Title = "Player 1";
                    break;
                }
                else if (connectedPlayer == '2')
                {
                    playerNumber = 2;
                    Console.Title = "Player 2";
                    break;
                }
                else
                {
                    Console.Title = "Viewer";
                    Console.WriteLine("Viewing mode. (Unable to control any players)");
                    break;
                }

            }

            

            while (true)
            {
                byte[] buffer = new byte[100];
                int bufferCount = netStream.Read(buffer);
                int usedPlayer = 0;

                if (buffer[0] == '1')
                    usedPlayer = 1;
                else if (buffer[0] == '2')
                    usedPlayer = 2;

                /*
                for (int i = 0; i < bufferCount; i++)
                {
                    Console.Write(Convert.ToChar(buffer[i]));
                }
                Console.WriteLine();
                */

                try
                {
                    if (buffer[1] == 'p')
                    {
                        float xPos = 0;
                        float yPos = 0;
                        string workingString = "";

                        for (int i = 2; i < bufferCount; i++)
                        {
                            char workingCharacter = Convert.ToChar(buffer[i]);

                            if (workingCharacter != ',' && workingCharacter != '.')
                                workingString += workingCharacter;
                            else if (workingCharacter == '.')
                            {
                                yPos = int.Parse(workingString); // TODO: System.FormatException: 'Input string was not in a correct format.'
                                break;
                            }
                            else
                            {
                                xPos = int.Parse(workingString);
                                workingString = "";
                            }
                        }

                        if (usedPlayer == 1)
                            player1Position = new Vector2(xPos, yPos);
                        else if (usedPlayer == 2)
                            player2Position = new Vector2(xPos, yPos);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to parse pos buffer.");

                    for (int i = 0; i < bufferCount; i++)
                    {
                        Console.Write(Convert.ToChar(buffer[i]));
                    }
                    Console.WriteLine();

                    Console.WriteLine("Player 1: " + player1Position.ToString());
                    Console.WriteLine("Player 2: " + player2Position.ToString());
                }

            }
        }

        protected override void Update(GameTime gameTime)
        {

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeSinceLastStreamUpdate += deltaTime;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var kState = Keyboard.GetState();

            Vector2 moveDirection = new Vector2(Convert.ToInt32(kState.IsKeyDown(Keys.D)) - Convert.ToInt32(kState.IsKeyDown(Keys.A)), Convert.ToInt32(kState.IsKeyDown(Keys.S)) - Convert.ToInt32(kState.IsKeyDown(Keys.W)));

            
            if (moveDirection.Length() != 0)
            {
                moveDirection = Vector2.Normalize(moveDirection);

                currentVelocity += moveDirection * acceleration * deltaTime;

                currentVelocity.X = currentVelocity.X * frictionMultiplier;
                currentVelocity.Y = currentVelocity.Y * frictionMultiplier;


                if (playerNumber == 1)
                    player1Position += currentVelocity * deltaTime;
                else if (playerNumber == 2)
                    player2Position += currentVelocity * deltaTime;

            }
            else
            {
                if (currentVelocity.Length() > 0)
                    currentVelocity = currentVelocity * slowdownFrictionMultiplier;

                if (playerNumber == 1)
                    player1Position += currentVelocity * deltaTime;
                else if (playerNumber == 2)
                    player2Position += currentVelocity * deltaTime;

            }

            if (playerNumber == 1)
            {

                player1Position = Vector2.Round(player1Position);

                // X
                if (player1Position.X > _graphics.PreferredBackBufferWidth - player1Texture.Width / 2)
                {
                    player1Position.X = _graphics.PreferredBackBufferWidth - player1Texture.Width / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }
                else if (player1Position.X < player1Texture.Width / 2)
                {
                    player1Position.X = player1Texture.Width / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }

                // Y
                if (player1Position.Y > _graphics.PreferredBackBufferHeight - player1Texture.Height / 2)
                {
                    player1Position.Y = _graphics.PreferredBackBufferHeight - player1Texture.Height / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }
                else if (player1Position.Y < player1Texture.Height / 2)
                {
                    player1Position.Y = player1Texture.Height / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }

                try
                {
                    if (timeSinceLastStreamUpdate >= timeBetweenStreamUpdates)
                    {
                        netStream.Write(asen.GetBytes('p' + player1Position.X.ToString() + ',' + player1Position.Y.ToString() + '.'));
                    }
                }
                catch
                {
                    Console.SetWindowSize(150, 20);
                    Console.WriteLine();
                    Console.WriteLine("ERROR: Failed to write data to network stream, server most likely shut down or other player left.");
                    Console.WriteLine();
                    Window.Position = new Point(_graphics.PreferredBackBufferWidth * 2, _graphics.PreferredBackBufferHeight * 2);
                    Exit();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
            else if (playerNumber == 2)
            {
                player2Position = Vector2.Round(player2Position);

                // X
                if (player2Position.X > _graphics.PreferredBackBufferWidth - player2Texture.Width / 2)
                {
                    player2Position.X = _graphics.PreferredBackBufferWidth - player2Texture.Width / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }
                else if (player2Position.X < player2Texture.Width / 2)
                {
                    player2Position.X = player2Texture.Width / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }

                // Y
                if (player2Position.Y > _graphics.PreferredBackBufferHeight - player2Texture.Height / 2)
                {
                    player2Position.Y = _graphics.PreferredBackBufferHeight - player2Texture.Height / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }
                else if (player2Position.Y < player2Texture.Height / 2)
                {
                    player2Position.Y = player2Texture.Height / 2;
                    currentVelocity = currentVelocity * wallSlowdownMultiplier;
                }

                if (timeSinceLastStreamUpdate >= timeBetweenStreamUpdates)
                {
                    netStream.Write(asen.GetBytes('p' + player2Position.X.ToString() + ',' + player2Position.Y.ToString() + '.'));
                }
            }

            if (timeSinceLastStreamUpdate > timeBetweenStreamUpdates)
                timeSinceLastStreamUpdate = 0;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            if (playerNumber == 1)
            {
                _spriteBatch.Draw(player1Texture, player1Position, null, Color.LightCoral, 0f, new Vector2(player1Texture.Width / 2, player1Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
                _spriteBatch.Draw(player2Texture, player2Position, null, Color.White, 0f, new Vector2(player2Texture.Width / 2, player2Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            }
            else if (playerNumber == 2)
            {
                _spriteBatch.Draw(player1Texture, player1Position, null, Color.White, 0f, new Vector2(player1Texture.Width / 2, player1Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
                _spriteBatch.Draw(player2Texture, player2Position, null, Color.LightCoral, 0f, new Vector2(player2Texture.Width / 2, player2Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            }
            else
            {
                _spriteBatch.Draw(player1Texture, player1Position, null, Color.White, 0f, new Vector2(player1Texture.Width / 2, player1Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
                _spriteBatch.Draw(player2Texture, player2Position, null, Color.White, 0f, new Vector2(player2Texture.Width / 2, player2Texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
