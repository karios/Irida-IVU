using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;

namespace SenseWallVis
{
    /// <summary>
    /// This is the main type
    /// </summary>
    public class SenseWallVisApp : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //PrimitiveBatch priminiteBatch;
#if WINDOWS
        const int screenWidth = 1024;
        const int screenHeight = 768;
        const bool fullscreen = true;
#endif
#if WINDOWSPHONE
        const int screenWidth = 800;
        const int screenHeight = 480;
        const bool fullscreen = true;
#endif
        int listenPort = 32222;
        string ICUIPAddress = "129.194.70.58";

        //Textures
        Texture2D nodeTexture;
        Texture2D packetTexture;

        bool showBorder=false;
        bool showGrid = false;

        List<Node> nodes = new List<Node>();
        List<packet> packets = new List<packet>();
        int lastID = 1;
        
        SpriteFont sf;

        //Border
        private List<PrimitiveLine> borderLines = new List<PrimitiveLine>();

        //Grid
        private List<PrimitiveLine> gridLines = new List<PrimitiveLine>();

        //Async socket
        SynchronousSocketListener ssl = new SynchronousSocketListener();
        public List<string> commandsBuffer = new List<string>();
        public List<string> log = new List<string>();
        
        Thread t;

        string status = "IRIDA Visualizer v0.2 - Marios Karagiannis - TCS/Sensor Lab - University of Geneva";
        string status2 = "Irida Control Unit IP:";
        string status3 = "Last ICU packet:";
        string lastMessage = "Nothing";

        private void setStatus(string newStatus)
        {
            status = newStatus;
        }
        
        public SenseWallVisApp()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            
            
            base.Initialize();

            //init graphics

            utils.InitGraphicsMode(screenWidth, screenHeight, fullscreen, ref graphics);

            //init socket thread

            initThread();
            initBorderLines();
            initGrid(8); //this is the grid size for calibration if you are actually putting your sensors on a grid 

            readSettings();
            sendRegisterPacketToICU();
        }

        private void sendRegisterPacketToICU()
        {
            //UdpClient uc = new UdpClient(); 

            string text = "register " + listenPort.ToString();

            UdpSend.Instance.SendMessage(ICUIPAddress, 5000, text);

            
            //byte[] send_buffer = Encoding.ASCII.GetBytes(text);

            //uc.Send(send_buffer, send_buffer.Length, ICUIPAddress, 5000);


            //IPAddress serverAddr = IPAddress.Parse(ICUIPAddress);
            //IPEndPoint endPoint = new IPEndPoint(serverAddr, 5000);
            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //

            

            //sock.SendTo(send_buffer, endPoint);
        }

        private void readSettings()
        {
            // create reader & open file
            StreamReader tr = new StreamReader("settings.txt");

            // read a line of text
            while (!tr.EndOfStream)
            {
                string ln = tr.ReadLine();
                if (ln.Length > 2)
                { 
                    if (ln[0]!='/' && ln[1]!='/') //comments
                    {
                        if (ln.LastIndexOf("port") > -1)
                        {
                            listenPort = Convert.ToInt32(ln.Substring(ln.LastIndexOf(':')+1));
                        }
                        else
                            if (ln.LastIndexOf("icuip") > -1)
                            {
                                ICUIPAddress = ln.Substring(ln.LastIndexOf(':')+1);
                            }

                    }
                }
            }

            // close the stream
            tr.Close();
        }

        private void initThread()
        {
            ssl.initPort(listenPort);
            t = new Thread(() => ssl.StartListening(commandsBuffer)); // Kick off a new thread
            t.Start();
        }

        private void initBorderLines()
        {
            PrimitiveLine tempLine = new PrimitiveLine(GraphicsDevice);
            tempLine.AddVector(Vector2.Zero);
            tempLine.AddVector(new Vector2(screenWidth ,0));
            tempLine.AddVector(new Vector2(screenWidth, 0));
            tempLine.AddVector(new Vector2(screenWidth, screenHeight ));
            tempLine.AddVector(new Vector2(screenWidth, screenHeight ));
            tempLine.AddVector(new Vector2(0, screenHeight));
            tempLine.AddVector(new Vector2(0, screenHeight));
            tempLine.AddVector(Vector2.Zero);
            borderLines.Add(tempLine);
        }

        private void initGrid(int gridSize)
        {
            
            for (int i = 1; i < gridSize; i++)
            {
                PrimitiveLine tempLine = new PrimitiveLine(GraphicsDevice);
                tempLine.AddVector(new Vector2(0,i*screenHeight / gridSize));
                tempLine.AddVector(new Vector2(screenWidth, i * screenHeight / gridSize));
                gridLines.Add(tempLine);

                tempLine = new PrimitiveLine(GraphicsDevice);
                tempLine.AddVector(new Vector2(i * screenWidth / gridSize, 0));
                tempLine.AddVector(new Vector2(i * screenWidth / gridSize, screenHeight));
                gridLines.Add(tempLine);

            }
            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            sf=Content.Load<SpriteFont>("SpriteFont1");
            nodeTexture =Content.Load<Texture2D>("node");
            packetTexture = Content.Load<Texture2D>("packet");   
            
        }

        public void processCommands()
        {
            while (commandsBuffer.Count > 0)
            {
                processCommand(commandsBuffer[0]);
                commandsBuffer.RemoveAt(0);
            }
        }

        private void processCommand(string command)
        {
             if (command == "")
            {
                return;
            }

             lastMessage = command;

            string[] args = command.Split(' ');
            switch (args[0])
            {
                case "setBadge":
                    if (args.Count() > 2)
                    {
                        int existsID = findNodeByhexID(args[1]);
                        if (existsID > 0)
                        {
                            int c = args.Count() - 1;

                            string badgeNumber = args[2];
                            string finalInfo = "";
                            for (int i = 3; i <= c; i++)
                            {
                                finalInfo += args[i] + " ";
                            }

                            if (badgeNumber == "1")
                            {
                                nodes[existsID - 1].setBadge1(finalInfo);
                            }
                            else
                                if (badgeNumber == "2")
                                {
                                    nodes[existsID - 1].setBadge2(finalInfo);
                                }

                        }
                    }
                    break;

                case "setText":
                    if (args.Count() > 2)
                    {
                        int existsID = findNodeByhexID(args[1]);
                        if (existsID > 0)
                        {
                            int c=args.Count()-1;
                            
                            string finalInfo="";
                            for (int i=2;i<=c;i++)
                            {
                                finalInfo+=args[i]+" ";
                            }
                            nodes[existsID - 1].setInfo(finalInfo);
                        }
                    }
                    break;
                case "clear":

                    nodes.Clear();
                    packets.Clear();

                    break;
                case "heartBeat":
                    if (args.Count() > 1)
                    if (args[1]!="0")
                    {
                        int existsID = findNodeByhexID(args[1]);

                        if (existsID > 0)
                        {

                            if (args.Count() >= 3)
                            {
                                try
                                {
                                    nodes[existsID - 1].setPosition(new Vector2((float)Convert.ToDouble(args[2], CultureInfo.InvariantCulture.NumberFormat) * screenWidth - nodeTexture.Width / 2.0f, (float)Convert.ToDouble(args[3], CultureInfo.InvariantCulture.NumberFormat) * screenHeight - nodeTexture.Height / 2.0f));
                                    foreach (Node n in nodes)
                                    {
                                        n.clearNeighboursLines();
                                        n.updateNeighbours();
                                    }

                                }
                                catch
                                {
                                    //setStatus("Error in command arguments");
                                }
                            }
                            
                            nodes[existsID - 1].heartBeat();
                        }
                        else
                        {
                            if (args.Count() >= 3)
                            {
                                Node new_node = new Node(spriteBatch,nodeTexture,Vector2.Zero);

                                try
                                {
                                    new_node.setPosition(new Vector2((float)Convert.ToDouble(args[2], CultureInfo.InvariantCulture.NumberFormat) * screenWidth - nodeTexture.Width / 2.0f, (float)Convert.ToDouble(args[3], CultureInfo.InvariantCulture.NumberFormat) * screenHeight - nodeTexture.Height / 2.0f));
                                    //new_node.setPosition((float)Convert.ToDouble(args[2]), (float)Convert.ToDouble(args[3]));
                                    new_node.setID(lastID);
                                    new_node.setHexID(args[1]);
                                    new_node.setInfoFont(sf);
                                    new_node.setInfo(args[1]);
                                    new_node.setGraphicsDevice(GraphicsDevice);
                                    new_node.setColor(Color.Black);
                                    lastID++;
                                    nodes.Add(new_node);
                                }
                                catch
                                {
                                    //setStatus("Error in command arguments");
                                }
                            }
                        }
                    }


                    break;
                case "changeColor":
                    if (args.Count() > 4)
                    {
                        int existsID = findNodeByhexID(args[1]);

                        if (existsID > 0)
                        {
                            nodes[existsID - 1].setColor(new Color((float)Convert.ToDouble(args[2], CultureInfo.InvariantCulture.NumberFormat) / 255f, (float)Convert.ToDouble(args[3], CultureInfo.InvariantCulture.NumberFormat) / 255f, (float)Convert.ToDouble(args[4], CultureInfo.InvariantCulture.NumberFormat) / 255f));
                            //mustUpdateDisplays = true;
                        }

                    }

                    break;
                case "activateNode":
                    if (args.Count() > 1)
                    {
                        int existsID = findNodeByhexID(args[1]);

                        if (existsID > 0)
                        {
                            nodes[existsID - 1].enable();
                            
                        }
                    }
                    break;
                case "disactivateNode":
                    if (args.Count() > 1)
                    {
                        int existsID = findNodeByhexID(args[1]);

                        if (existsID > 0)
                        {
                            nodes[existsID - 1].disable();
                            
                        }
                    }
                    break;
                case "sendPacket":
                    if (args.Count() > 2)
                    {
                        int source = findNodeByhexID(args[1]);
                        int destination = findNodeByhexID(args[2]);
                        
                        if (source > 0 && destination > 0 && source!=destination && source<=nodes.Count && destination<=nodes.Count)
                        if (nodes[destination-1].isEnabled())
                        {
                            packet pckt;
                            if (args.Count() >3)
                            {
                                pckt = new packet(spriteBatch, packetTexture, nodes[source - 1], nodes[destination - 1],args[3],sf);
                            }
                            else
                            {
                                pckt = new packet(spriteBatch, packetTexture, nodes[source - 1], nodes[destination - 1]);
                            }
                            packets.Add(pckt);
                            //commandsBuffer.Add("addNeighbor " + args[1] + " " + args[2]);
                            
                        }
                    }
                    break;
                case "resetNeighbors":
                    if (args.Count() > 1)
                    {
                        int existsID = findNodeByhexID(args[1]);

                        if (existsID > 0)
                        {
                            nodes[existsID - 1].clearNeighbours();
                        }
                    }
                    break;
                case "addNeighbor":
                    if (args.Count() > 2)
                    {
                        int source = findNodeByhexID(args[1]);
                        int destination = findNodeByhexID(args[2]);
                        if (source > 0 && destination > 0)
                        {
                            nodes[source - 1].addNeighbour(nodes[destination - 1]);
                        }
                    }
                    break;
            }
        }

        private int findNodeByhexID(string hexID)
        {
            int res = -1;
            foreach (Node nd in nodes)
            {
                if (nd.getIDHex() == hexID)
                {
                    res = nd.getID();
                    break;
                }
            }
            return res;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            KeyboardState keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.Escape))
            {
                
                SynchronousSocketListener.keepGoing = false;
                //t.Join();
                
                Exit();
            }

            
            if (keys.IsKeyDown(Keys.A))
            {
                foreach (Node n in nodes)
                {
                    n.heartBeat();
                }
            }
            
            if (keys.IsKeyDown(Keys.Space)) //just for debugging
            {
                commandsBuffer.Add("heartBeat A 0.3 0.5");
                commandsBuffer.Add("heartBeat B 0.6 0.5");
                commandsBuffer.Add("changeColor A 1 0 0");
                commandsBuffer.Add("changeColor B 0 0 1");
            }


            //calibration 
            if (keys.IsKeyDown(Keys.L))
            {
                showBorder = true;
            }
            if (keys.IsKeyDown(Keys.K))
            {
                showBorder = false;
            }

            if (keys.IsKeyDown(Keys.O))
            {
                showGrid = true;
            }
            if (keys.IsKeyDown(Keys.I))
            {
                showGrid = false;
            }

            
            if (keys.IsKeyDown(Keys.Q))
            {
                foreach (Node n in nodes)
                {
                    n.disable();
                }
            }


            if (keys.IsKeyDown(Keys.W)) 
            {
                foreach (Node n in nodes)
                {
                    n.enable();
                }
            }
            

            foreach (Node n in nodes)
            {
                n.update();
            }

            foreach (packet p in packets)
            {
                p.update();
            }

            for (int i = packets.Count; i > 0; i--)
            {
                if (packets[i - 1].getMovementPercentage() >= 1f)
                {
                    packets.RemoveAt(i - 1);
                }
            }

            processCommands();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            foreach (Node n in nodes)
            {
                n.draw();
            }

            foreach (packet p in packets)
            {
                p.draw();
            }

            if (showBorder)
            {
                foreach (PrimitiveLine pm in borderLines)
                {
                    pm.Render(spriteBatch,1);
                }
            }

            if (showGrid)
            {
                foreach (PrimitiveLine pm in gridLines)
                {
                    pm.Render(spriteBatch,1);
                }
            }

            spriteBatch.DrawString(sf, status, Vector2.Zero, Color.Black);
            spriteBatch.DrawString(sf, status2 + ICUIPAddress, new Vector2(0, 25), Color.Black);
            try
            {

                spriteBatch.DrawString(sf, status3 + lastMessage, new Vector2(0, 50), Color.Black);
            }
            catch (Exception e)
            {
                spriteBatch.DrawString(sf, status3 + "Invalid Message", new Vector2(0, 50), Color.Black);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
