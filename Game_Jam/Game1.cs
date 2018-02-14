using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game_Jam
{
    internal class GameData
    {
        public GameData(float z, Vector2 playerStartPos, Rect Bounds)
        {
            this.player = default(Player);
            this.player.Init(z, playerStartPos);
            this.bounds = Bounds;
        }
        
        public void Update(float deltatime)
        {
            this.dt = deltatime;
            Vector2 PlayerPrevPos = this.player.Pos;
            this.player.Update(this.dt);
            bool flag = this.player.GetDrawRectangle().min.Y <= this.bounds.min.Y;
            if (flag)
            {
                this.player.Pos.Y = PlayerPrevPos.Y;
                this.player.OnGround = true;
                this.player.Jumping = false;
            }
            bool flag2 = this.player.GetDrawRectangle().min.X <= this.bounds.min.X;
            if (flag2)
            {
                this.player.Pos.X = PlayerPrevPos.X;
            }
        }
        
        public Player player;
        
        private float dt;
        
        public int CurrentRoom = 0;
        
        public Rect bounds;
    }
    public enum TextureID
    {
        font,
        pixel,
        background,
        player_left,
        player_right,
        MAX_TEXTURES
    }
    public struct Player
    {
        public void Init(float z, Vector2 pos)
        {
            this.Dim = new Vector2(16f, 23f);
            this.tex_id = TextureID.player_right;
            this.Direction = FacingDirection.Right;
            this.Pos = pos;
            this.Zindex = z;
            this.OnGround = false;
            this.Jumping = false;
            this.acc = 200f;
            this.drag = 0.009f;
        }
        
        public Rect GetDrawRectangle()
        {
            return new Rect(this.Pos - this.Dim / 2f, this.Dim);
        }
        
        public void MoveLeft()
        {
            this.Direction = FacingDirection.Left;
            this.ddPos = -Vector2.UnitX;
        }
        
        public void MoveTo()
        {
        }
        
        public void Jump(float height)
        {
            bool flag = this.OnGround && !this.Jumping;
            if (flag)
            {
                this.ddPos = Vector2.UnitY * height;
                this.OnGround = false;
                this.Jumping = true;
            }
        }
        public void MoveRight()
        {
            this.Direction = FacingDirection.Right;
            this.ddPos = Vector2.UnitX;
        }
        
        public void Update(float dt)
        {
            bool gravity = true;
            bool flag = gravity;
            if (flag)
            {
                this.ddPos.Y = this.ddPos.Y + -0.5f;
            }
            Vector2 OlddPos = this.dPos;
            this.dPos += this.ddPos * this.acc * dt;
            this.Pos += (this.dPos + OlddPos) * 0.5f * dt;
            this.ddPos = Vector2.Zero;
            this.dPos *= 1f - this.drag;
            switch (this.Direction)
            {
                case FacingDirection.Right:
                    this.tex_id = TextureID.player_right;
                    break;
                case FacingDirection.Left:
                    this.tex_id = TextureID.player_left;
                    break;
            }
        }

        public Vector2 Pos;
        public Vector2 dPos;
        
        public Vector2 ddPos;
        
        public Vector2 Dim;
        
        public float Zindex;
        
        public FacingDirection Direction;
        
        public TextureID tex_id;
        
        public float drag;
        
        public float acc;
        
        public bool OnGround;
        public bool Jumping;
    }
    public class Game1 : Game
    {
        private void LoadTexture(string str)
        {
            this.Textures_2D.Add(base.Content.Load<Texture2D>(str));
        }

        private Texture2D GetPixelTexture(Color col)
        {
            Texture2D tex = new Texture2D(base.GraphicsDevice, 1, 1);
            tex.SetData<Color>(new Color[]
            {
                col
            });
            return tex;
        }

        public Game1()
        {
            base.IsMouseVisible = true;
            this.graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 600
            };
            base.Content.RootDirectory = "Content";
            base.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        private void InitializeTextures()
        {
            this.Textures_2D = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
            {
                TextureID t = (TextureID)i;
                bool flag = t == TextureID.pixel;
                if (flag)
                {
                    this.Textures_2D.Add(this.GetPixelTexture(Color.White));
                }
                else
                {
                    bool flag2 = t == TextureID.font;
                    if (flag2)
                    {
                        this.font = base.Content.Load<SpriteFont>("Font");
                        this.Textures_2D.Add(this.font.Texture);
                    }
                    else
                    {
                        this.LoadTexture(t.ToString());
                    }
                }
            }
        }

        protected override void LoadContent()
        {
            this.RealScreenDim = new Vector2((float)base.GraphicsDevice.Viewport.Width, (float)base.GraphicsDevice.Viewport.Height);
            this.ScreenDim = new Vector2(1920f, 1080f) * 2f;
            this.DrawList = new List<DrawCommand>();
            this.spriteBatch = new SpriteBatch(base.GraphicsDevice);
            this.pixel = this.GetPixelTexture(Color.White);
            this.InitializeTextures();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool flag = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape);
            if (flag)
            {
                base.Exit();
            }
            this.PrevMouseState = this.MouseState;
            this.PrevKeyboardState = this.KeyboardState;
            this.KeyboardState = Keyboard.GetState();
            this.MouseState = Mouse.GetState();
            Vector2 MousePos = new Vector2((float)this.MouseState.X, (float)(base.GraphicsDevice.Viewport.Height - this.MouseState.Y)) / this.RealScreenDim * this.ScreenDim;
            Vector2 PrevMousePos = new Vector2((float)this.PrevMouseState.X, (float)(base.GraphicsDevice.Viewport.Height - this.PrevMouseState.Y)) / this.RealScreenDim * this.ScreenDim;
            Vector2 MousedPos = MousePos - PrevMousePos;
            float heightfactor = 0.4f;
            int pad = 30;
            Rect bottomview = new Rect(0f, 0f, this.ScreenDim.X, this.ScreenDim.Y * heightfactor);
            bool flag2 = !this.GameInit;
            if (flag2)
            {
                this.gameData = new GameData(2f, new Vector2(100f, 150f), new Rect(Vector2.Zero, bottomview.Dim));
                GameData gameData = this.gameData;
                gameData.player.Dim = gameData.player.Dim * 10f;
                this.gameData.player.acc = 7500f;
                this.GameInit = true;
            }
            Rect border = bottomview;
            border.max.X = border.max.X - border.Dim.X * 0.9f;
            border = bottomview;
            border.min.X = border.min.X + border.Dim.X * 0.9f;
            border = bottomview;
            border.max.Y = border.max.Y - border.Dim.Y * 0.9f;
            border = bottomview;
            border.min.Y = border.min.Y + border.Dim.X * 0.9f;
            bottomview.Inflate((float)(-(float)pad));
            bool flag3 = this.KeyboardState.IsKeyDown(Keys.Left);
            if (flag3)
            {
                this.gameData.player.MoveLeft();
            }
            bool flag4 = this.KeyboardState.IsKeyDown(Keys.Right);
            if (flag4)
            {
                this.gameData.player.MoveRight();
            }
            bool flag5 = this.PrevKeyboardState.IsKeyUp(Keys.Space) && this.KeyboardState.IsKeyDown(Keys.Space);
            if (flag5)
            {
                this.gameData.player.Jump(200f);
            }
            bool flag6 = this.KeyboardState.IsKeyUp(Keys.Z) && this.PrevKeyboardState.IsKeyDown(Keys.Z);
            if (flag6)
            {
                this.starttextanim = !this.starttextanim;
                this.timer = 0f;
            }
            bool flag7 = this.setmovepoint;
            if (flag7)
            {
                Vector2 target = this.movetopoint - this.gameData.player.Pos;
                bool flag8 = target.X < 0f;
                if (flag8)
                {
                    this.gameData.player.MoveLeft();
                }
                else
                {
                    this.gameData.player.MoveRight();
                }
            }
            this.gameData.Update(dt);
            bool flag9 = this.gameData.player.Pos.X < this.movetopoint.X + 2f && this.gameData.player.Pos.X > this.movetopoint.X - 2f;
            if (flag9)
            {
                this.setmovepoint = false;
            }
            Rect playerdrawrect = this.gameData.player.GetDrawRectangle();
            this.gameData.CurrentRoom = (int)(playerdrawrect.max.X / this.gameData.bounds.max.X);
            playerdrawrect.min.X = playerdrawrect.min.X % this.gameData.bounds.max.X;
            playerdrawrect.max.X = playerdrawrect.max.X % this.gameData.bounds.max.X;
            playerdrawrect.min += bottomview.min;
            float scrollfactor = playerdrawrect.min.X;
            Rect Backgroundrect = bottomview;
            Backgroundrect.max.X = Backgroundrect.max.X + this.gameData.bounds.max.X * 0.9f;
            Backgroundrect.min.X = Backgroundrect.min.X + this.gameData.bounds.max.X * 0.9f;
            Rect Backgroundrect2 = bottomview;
            Backgroundrect.max.X = Backgroundrect.max.X - playerdrawrect.min.X;
            Backgroundrect.min.X = Backgroundrect.min.X - playerdrawrect.min.X;
            Backgroundrect2.max.X = Backgroundrect2.max.X - playerdrawrect.min.X;
            Backgroundrect2.min.X = Backgroundrect2.min.X - playerdrawrect.min.X;
            this.DrawList.Add(new DrawCommand(this.gameData.player.tex_id, this.gameData.player.Zindex, playerdrawrect, Color.White));
            this.DrawList.Add(new DrawCommand(TextureID.background, this.gameData.player.Zindex - 0.5f, Backgroundrect, Color.White));
            this.DrawList.Add(new DrawCommand(TextureID.background, this.gameData.player.Zindex - 0.5f, Backgroundrect2, Color.White));
            bool flag10 = this.starttextanim;
            if (flag10)
            {
                this.timer += dt;
            }
            Rect topview = new Rect(0f, this.ScreenDim.Y * heightfactor, this.ScreenDim.X, this.ScreenDim.Y * (1f - heightfactor));
            this.DrawList.Add(new DrawCommand(TextureID.pixel, 0f, topview, Color.BlueViolet));
            topview.Inflate((float)(-(float)pad));
            this.DrawList.Add(new DrawCommand(TextureID.background, 1f, topview, Color.White));
            Vector2 fontscale = new Vector2(3f);
            Vector2 fontdim = this.font.MeasureString("A");
            Vector2 startpad = new Vector2(20f, -20f);
            Vector2 startpos = topview.TopLeft + startpad + new Vector2(0f, -fontdim.Y * fontscale.Y);
            Vector2 textpos = startpos;
            float textduration = 2f;
            this.Labels.Clear();
            this.Labels.Add(new Game1.Label("Hello", null));
            this.Labels.Add(new Game1.Label(" ", null));
            this.Labels.Add(new Game1.Label("World", null));
            this.Labels.Add(new Game1.Label("NL", null));
            this.Labels.Add(new Game1.Label("Embarking on an adventure!!!!", null));
            this.Labels.Add(new Game1.Label("NL", null));
            this.Labels.Add(new Game1.Label("Current Room : " + this.gameData.CurrentRoom, null));
            this.Labels.Add(new Game1.Label("NL", null));
            int movedistance = 400;
            this.Labels.Add(new Game1.Label("Click to move to player right (" + movedistance + ")", delegate
            {
                this.movetopoint = this.gameData.player.Pos + new Vector2((float)movedistance, 0f);
                this.setmovepoint = true;
            }));
            this.Labels.Add(new Game1.Label("NL", null));
            this.Labels.Add(new Game1.Label("Click to move to player left (" + movedistance + ")", delegate
            {
                this.movetopoint = this.gameData.player.Pos - new Vector2((float)movedistance, 0f);
                this.setmovepoint = true;
            }));
            for (int i = 0; i < this.Labels.Count; i++)
            {
                Game1.Label lbl = this.Labels[i];
                bool flag11 = lbl.str == "NL";
                if (flag11)
                {
                    textpos.Y -= fontdim.Y * fontscale.Y;
                    textpos.X = startpos.X;
                }
                else
                {
                    int numchars = Math.Min((int)Math.Round((double)(this.timer / (textduration / (float)lbl.str.Length))), lbl.str.Length);
                    string chars = lbl.str.Substring(0, numchars);
                    Rect strrect = new Rect(textpos, this.font.MeasureString(chars) * fontscale);
                    this.DrawList.Add(new DrawCommand(chars, 1.5f, strrect, Color.White));
                    bool flag12 = strrect.Contains(MousePos);
                    if (flag12)
                    {
                        bool flag13 = chars != " ";
                        if (flag13)
                        {
                            this.DrawList.Add(new DrawCommand(TextureID.pixel, 1.4999f, strrect, Color.Green));
                        }
                        bool flag14 = this.MouseState.LeftButton == ButtonState.Released && this.PrevMouseState.LeftButton == ButtonState.Pressed;
                        if (flag14)
                        {
                            Game1.Label.OnMouseClick onClick = lbl.OnClick;
                            if (onClick != null)
                            {
                                onClick();
                            }
                        }
                    }
                    textpos.X += strrect.Dim.X;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.GraphicsDevice.Clear(Color.Brown);
            this.DrawList.Sort(delegate (DrawCommand x, DrawCommand y)
            {
                bool flag3 = x.Zindex > y.Zindex;
                int result;
                if (flag3)
                {
                    result = 1;
                }
                else
                {
                    bool flag4 = y.Zindex > x.Zindex;
                    if (flag4)
                    {
                        result = -1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                return result;
            });
            List<DrawCommand> SortedDrawList = (from x in this.DrawList
                                                group x by x.tex_id).SelectMany((IGrouping<TextureID, DrawCommand> x) => x.ToList<DrawCommand>()).ToList<DrawCommand>();
            SortedDrawList.Sort(delegate (DrawCommand x, DrawCommand y)
            {
                bool flag3 = x.Zindex > y.Zindex;
                int result;
                if (flag3)
                {
                    result = 1;
                }
                else
                {
                    bool flag4 = y.Zindex > x.Zindex;
                    if (flag4)
                    {
                        result = -1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                return result;
            });
            Vector2 Scale = this.RealScreenDim / this.ScreenDim;
            this.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
            for (int i = 0; i < SortedDrawList.Count; i++)
            {
                DrawCommand cmd = SortedDrawList[i];
                bool flag = cmd.SourceRectangle == Rect.Zero;
                if (flag)
                {
                    Rectangle src = this.Textures_2D[(int)cmd.tex_id].Bounds;
                    cmd.SourceRectangle = new Rect((float)src.Y, (float)src.X, (float)src.Width, (float)src.Height);
                }
                float minY = this.ScreenDim.Y - cmd.DestRectangle.min.Y;
                float maxY = this.ScreenDim.Y - cmd.DestRectangle.max.Y;
                bool flag2 = maxY > minY;
                if (flag2)
                {
                    cmd.DestRectangle.min.Y = minY;
                    cmd.DestRectangle.max.Y = maxY;
                }
                else
                {
                    cmd.DestRectangle.min.Y = maxY;
                    cmd.DestRectangle.max.Y = minY;
                }
                cmd.DestRectangle *= Scale;
                cmd.DestRectangle.min.X = (float)Math.Round((double)cmd.DestRectangle.min.X);
                cmd.DestRectangle.max.X = (float)Math.Round((double)cmd.DestRectangle.max.X);
                cmd.DestRectangle.min.Y = (float)Math.Round((double)cmd.DestRectangle.min.Y);
                cmd.DestRectangle.max.Y = (float)Math.Round((double)cmd.DestRectangle.max.Y);
                Vector2 strscaledim = cmd.DestRectangle.Dim / this.font.MeasureString(cmd.str);
                float stringscale = Math.Min(strscaledim.X, strscaledim.Y);
                switch (cmd.Type)
                {
                    case DrawCommandType.DrawString:
                        this.spriteBatch.DrawString(this.font, cmd.str, cmd.DestRectangle.BottomLeft, cmd.tint, 0f, Vector2.Zero, strscaledim, SpriteEffects.None, 0f);
                        break;
                    case DrawCommandType.DrawTexture:
                        this.spriteBatch.Draw(this.Textures_2D[(int)cmd.tex_id], cmd.DestRectangle.ToRectangle(), new Rectangle?(cmd.SourceRectangle.ToRectangle()), cmd.tint);
                        break;
                }
            }
            this.spriteBatch.End();
            this.DrawList.Clear();
            base.Draw(gameTime);
        }

        private GraphicsDeviceManager graphics;

        private SpriteBatch spriteBatch;

        private Vector2 RealScreenDim;

        private Vector2 ScreenDim;

        private Texture2D pixel;

        private SpriteFont font;

        private List<DrawCommand> DrawList;

        private List<Texture2D> Textures_2D;

        private MouseState MouseState;

        private KeyboardState KeyboardState;

        private MouseState PrevMouseState;

        private KeyboardState PrevKeyboardState;

        private List<Game1.Label> Labels = new List<Game1.Label>();

        private bool GameInit = false;

        private GameData gameData;

        private float timer;

        private bool starttextanim = true;

        private Vector2 movetopoint = Vector2.Zero;

        private bool setmovepoint;

        public struct Label
        {
            public Label(string strs, Game1.Label.OnMouseClick del = null)
            {
                this.str = strs;
                this.OnClick = del;
            }

            public string str;

            public Game1.Label.OnMouseClick OnClick;

            public delegate void OnMouseClick();
        }
    }
}
