using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SenseWallVis
{
    class Node
    {
        private Vector2 position;
        private Color color;
        private Color newColor;
        private float targetIntensity = 1;
        private float currentIntensity = 0;

        private float targetHeartBeatIntensity = 0;
        private float heartBeatIntensity = 0;

        private Texture2D currentTexture;
        private SpriteBatch sb;

        private SpriteFont sf;
        private GraphicsDevice gd;

        private string info="";
        private string badge1 = "";
        private string badge2 = "";
        private int ID=0;
        private string hexID="Node";

        public struct nS
        {
            public Node neighbor;
            public float timer;
        }

        private List<PrimitiveLine> neighbours = new List<PrimitiveLine>();
        private List<nS> neighboursNodes = new List<nS>();

        public void clearNeighbours()
        {
            neighboursNodes.Clear();
            clearNeighboursLines();
        }

        public void clearNeighboursLines()
        {
            neighbours.Clear();
        }

        public void addNeighbour(Node newNeighbour)
        {
            nS newN=new nS();
            newN.neighbor = newNeighbour;
            newN.timer = 1;
            neighboursNodes.Add(newN);

            PrimitiveLine tempLine = new PrimitiveLine(gd);
/*
            if (getPosition().X == newNeighbour.getPosition().X)
            {
                tempLine.AddVector(getPosition() + new Vector2(5, 0));
                tempLine.AddVector(newNeighbour.getPosition());

                tempLine.AddVector(getPosition() + new Vector2(-5, 0));
                tempLine.AddVector(newNeighbour.getPosition());
            }
            else
            {

                tempLine.AddVector(getPosition()+new Vector2(0,5));
                tempLine.AddVector(newNeighbour.getPosition());

                tempLine.AddVector(getPosition() + new Vector2(0, -5));
                tempLine.AddVector(newNeighbour.getPosition());
            }*/

            //Simple line
            tempLine.AddVector(getPosition());
            tempLine.AddVector(newNeighbour.getPosition());


            neighbours.Add(tempLine);
            
        }

        public void updateNeighbours()
        {
            clearNeighboursLines();

            foreach (nS n in neighboursNodes)
            {
                PrimitiveLine tempLine = new PrimitiveLine(gd);


                /*

                if (getPosition().X == n.neighbor.getPosition().X)
                {
                    tempLine.AddVector(getPosition() + new Vector2(5, 0));
                    tempLine.AddVector(n.neighbor.getPosition());

                    tempLine.AddVector(getPosition() + new Vector2(-5, 0));
                    tempLine.AddVector(n.neighbor.getPosition());
                }
                else
                {

                    tempLine.AddVector(getPosition() + new Vector2(0, 5));
                    tempLine.AddVector(n.neighbor.getPosition());

                    tempLine.AddVector(getPosition() + new Vector2(0, -5));
                    tempLine.AddVector(n.neighbor.getPosition());
                }*/

                //Simple line
                tempLine.AddVector(getPosition());
                tempLine.AddVector(n.neighbor.getPosition());
                
                
                neighbours.Add(tempLine);
            }

        }

        public void setGraphicsDevice(GraphicsDevice newGD)
        {
            gd = newGD;
        }

        public void heartBeat()
        {
            targetHeartBeatIntensity = 1;
            enable();
            //heartBeatIntensity = 0;
        }

        public void setID(int newID)
        {
            ID = newID;
        }

        public void setHexID(string newHexID)
        {
            hexID = newHexID;
        }

        public string getIDHex()
        {
            return hexID;
        }

        public int getID()
        {
            return ID;
        }

        public void setInfo(string newInfo)
        {
            info = newInfo;
        }

        public void setBadge1(string newBadge)
        {
            badge1 = newBadge;
        }

        public void setBadge2(string newBadge)
        {
            badge2 = newBadge;
        }

        public void setInfoFont(SpriteFont newSF)
        {
            sf = newSF;
        }

        public void setColor(Color nColor)
        {
            newColor = nColor;
        }

        public void disable()
        {
            targetIntensity = 0.1f;
        }

        public void enable()
        {
            targetIntensity = 1f;
        }

        public Node(SpriteBatch initSB, Texture2D initTexture, Vector2 initPosition)
        {
            color = Color.White;
            newColor = Color.White;
            position = initPosition;
            currentTexture = initTexture;
            sb = initSB;
        }

        public Node(SpriteBatch initSB, Texture2D initTexture, Vector2 initPosition, Color initColor)
        {
            color = initColor;
            newColor = initColor;
            position = initPosition;
            currentTexture = initTexture;
            sb = initSB;
        }

        public Vector2 getPosition()
        {
            return new Vector2(position.X+currentTexture.Width/2.0f,position.Y+currentTexture.Height/2.0f);
        }

        public void setPosition(Vector2 newPosition)
        {
            position = newPosition;
            foreach (PrimitiveLine pm in neighbours)
            {
                pm.getVectors()[0] = getPosition();
            }
        }

        public void update()
        {
            if (currentIntensity != targetIntensity)
            {
                currentIntensity += (targetIntensity - currentIntensity) / 40;
            }

            if (heartBeatIntensity < targetHeartBeatIntensity)
            {
                heartBeatIntensity += (targetHeartBeatIntensity - heartBeatIntensity) / 30;
                if (heartBeatIntensity > 0.95 * targetHeartBeatIntensity)
                {
                    heartBeatIntensity = 0;
                    targetHeartBeatIntensity = 0;
                }
            }

            if (newColor.R != color.R || newColor.G != color.G || color.B != newColor.B)
            {
                if (color.R > newColor.R)
                {
                    color.R -= (byte)((color.R - newColor.R) / 20);
                }
                    
                else if (color.R < newColor.R)
                {
                    color.R += (byte)((color.R - newColor.R) / 20);
                }
                if (color.G > newColor.G)
                {
                    color.G -= (byte)((color.G - newColor.G) / 20);
                }
                else if (color.G < newColor.G)
                {
                    color.G += (byte)((color.G - newColor.G) / 20);
                }
                if (color.B > newColor.B)
                {
                    color.B -= (byte)((color.B - newColor.B) / 20);
                }
                else if (color.B < newColor.B)
                {
                    color.B += (byte)((color.B - newColor.B) / 20);
                }
            }
            else
            {
                newColor = color;
            }

            for (int i = neighboursNodes.Count-1; i >=0; i--)
            {
                nS n = neighboursNodes[i];
                //n.timer -= 0.0003f;  //neighbours links fade
                neighboursNodes[i] = n;
                
                if (n.timer < 0)
                {
                    neighboursNodes.RemoveAt(i);
                    neighbours.RemoveAt(i);
                }
            }


            if (targetIntensity <= 0.1f)
            {
                targetIntensity = 0.1f;
                disable();

            }
            else
            {
                targetIntensity -= 0.001f;
            }
        }

        public bool isEnabled()
        {
            return targetIntensity > 0.5f;
        }

        public void draw()
        {
            
            //draw node
            sb.Draw(currentTexture, position, color * currentIntensity);
            //draw heartbeat 
            if (heartBeatIntensity > 0)
            {
                sb.Draw(currentTexture, 
                    new Vector2(position.X + currentTexture.Width /2.0f, position.Y + currentTexture.Height /2.0f),
                    new Rectangle(0, 0, currentTexture.Width, currentTexture.Height), 
                    color * (1 - heartBeatIntensity), 
                    0, 
                    new Vector2(currentTexture.Width/2.0f, currentTexture.Height/2.0f), 
                    heartBeatIntensity * 3.0f, 
                    SpriteEffects.None, 
                    0);
            }
            int i = 0;
            //draw connections
            foreach (PrimitiveLine pm in neighbours)
            {

                pm.Render(sb, neighboursNodes[i].timer);
                i++;
            }


            //draw info text
            if (info != "")
            {
                sb.DrawString(sf, info, getPosition() + new Vector2(-sf.MeasureString(info).X / 2.0f, currentTexture.Height / 2.0f + sf.MeasureString(info).Y/2.0f), color);
            }

            //draw badges
            if (badge1 != "")
            {
                sb.DrawString(sf, badge1, getPosition() + new Vector2(-sf.MeasureString(badge1).X*2, -currentTexture.Height / 2.0f - sf.MeasureString(badge1).Y), color);
            }

            if (badge2 != "")
            {
                sb.DrawString(sf, badge2, getPosition() + new Vector2(sf.MeasureString(badge2).X, -currentTexture.Height / 2.0f - sf.MeasureString(badge2).Y), color);
            }
            
        }
    }
}
