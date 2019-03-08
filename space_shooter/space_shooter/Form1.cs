using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace space_shooter
{
    public partial class Form1 : Form
    {
        Timer t;
        List<Game_object> everything = new List<Game_object>();
        Myself myship = new Myself(Image.FromFile(Application.StartupPath + @"\images\my_ship.png"), object_type.myself);
        Graphics g;
        Bitmap bmp;
        int level = 1;
        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            //everything.Add(myship);
            bmp = new Bitmap(pb.Width, pb.Height);
            g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            t = new Timer();
            t.Interval = 10;
            t.Tick += render;
            myship.scene_size = pb.Size;
        }

        private void render(object sender, EventArgs e)
        {
            g.Clear(Color.Transparent);
            myship.Draw(g, myship.location, everything, level);
            if(level < 2)
            {
                game_control();
                level++;
            }
            for (int i = 0; i < everything.Count; i++)
            {
                everything[i].Draw(g, everything[i].location, everything, level);
                everything[i].Update(everything);
            }
            pb.Image = bmp;
        }

        void game_control()
        {
            for(int i = 0; i < level * 5; i++)
            {
                Enemy en = new Enemy(enemy_type.large, Image.FromFile(Application.StartupPath + @"\images\enemy3.png"), object_type.enemy);
                en.location = new Point(rnd.Next(1, pb.Width), 0);
                en.speed = rnd.Next(1, 5);
                everything.Add(en);
            }
        }

        private void pb_MouseDown(object sender, MouseEventArgs e)
        {
            myship.Shoot(everything);
        }

        private void pb_MouseMove(object sender, MouseEventArgs e)
        {
            myship.location.X = e.Location.X - myship.size.Width / 2;
            myship.location.Y = e.Location.Y - myship.size.Height / 2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            t.Start();
        }
    }

    public enum enemy_type
    {
        small = 0,
        medium = 1,
        large = 2,
        main = 3,
    }

    public enum object_type
    {
        myself = 0,
        enemy = 1,
        bullet = 2,
    }
    /// <summary>
    /// Every game object inherits from this class
    /// </summary>
    public class Game_object
    {
        public Point location;
        public Size size;
        public bool alive = true;
        public int speed = 1;
        public int bullet_speed = 1;
        public bool can_shoot = false;
        public Graphics G;
        public Stopwatch sw = new Stopwatch();
        public Size scene_size;
        public Image im;
        public object_type mytype;

        public Game_object(Image image, object_type type)
        {
            this.mytype = type;
            this.im = image;
            this.size = image.Size;
            sw.Start();
        }

        public void Draw(Graphics g, Point new_location, List<Game_object> others, int level)
        {
            this.G = g;
            if(alive)
            {
                g.DrawImage(im, this.location);
                Move(new_location, others, level);
            }
            else
            {
                Dispose(others);
            }
        }

        public virtual void Move(Point new_location, List<Game_object> others, int level)
        {
            this.speed *= level;
            this.bullet_speed *= level;
            this.location = new_location;
        }

        public void Shoot(Graphics g, Image image)
        {

        }

        public virtual void Update(List<Game_object> everything)
        {

        }

        public void Dispose(List<Game_object> everything)
        {
            everything.Remove(this);
        }
    }

    public class Myself : Game_object
    {
        public Myself(Image image, object_type type) : base(image, type)
        {
            this.can_shoot = true;
            this.bullet_speed = 20;
        }

        public override void Move(Point new_location, List<Game_object> others, int level)
        {
            this.location = new_location;
        }

        public void Shoot(List<Game_object> everything)
        {
            Bullet bullet = new Bullet(this.location, this.size, this.bullet_speed, Image.FromFile(Application.StartupPath + @"\images\bullet.png"), object_type.bullet);
            everything.Add(bullet);
        }
    }

    public class Enemy : Game_object
    {
        public int shooting_frequency = 1;
        public Enemy(enemy_type etype, Image image, object_type type) : base(image, type)
        {
            switch(etype)
            {
                case enemy_type.small:
                    {
                        this.can_shoot = false;
                        this.bullet_speed = 0;
                        this.speed = 2;
                        break;
                    }
                case enemy_type.medium:
                    {
                        this.can_shoot = true;
                        this.bullet_speed = 5;
                        shooting_frequency = 3;
                        this.speed = 3;
                        break;
                    }
                case enemy_type.large:
                    {
                        this.can_shoot = true;
                        this.bullet_speed = 10;
                        shooting_frequency = 2;
                        this.speed = 4;
                        break;
                    }
                case enemy_type.main:
                    {
                        this.can_shoot = true;
                        this.bullet_speed = 15;
                        shooting_frequency = 1;
                        this.size = new Size(40, 40);
                        this.speed = 5;
                        break;
                    }
            }
        }

        public override void Move(Point new_location, List<Game_object> others, int level)
        {
            this.location.Y += this.speed;
            this.location.X = new_location.X;
            //enemies shoot automatically at a certain frequency
            if((base.sw.ElapsedMilliseconds / 1000) % shooting_frequency == 0)
            {
                Shoot();
            }
            this.alive = Collide(others);
        }

        public void Shoot()
        {
            Bullet bullet = new Bullet(this.location, this.size, this.bullet_speed, Image.FromFile(Application.StartupPath + @"\images\bullet.png"), object_type.bullet);
        }

        public bool Collide(List<Game_object> others)
        { 
            if(others.Where(o => o.mytype == object_type.bullet).Where(o1 => o1.location.X + o1.size.Width >= this.location.X && o1.location.X <= this.location.X - this.size.Width).Any(o2 => o2.location.Y + o2.size.Height >= this.location.Y))
            {
                return false;
            }
            return true;
            //if (others.Where(o => o.location.X + o.size.Width >= this.location.X && o.location.X <= this.location.X - this.size.Width).Any(o => o.location.Y + o.size.Height >= this.location.Y))
            //{
            //    return false;
            //}
            //return true;
        }
    }

    public class Bullet : Game_object
    {
        int my_speed = 1;
        public Bullet(Point Location, Size shooter_size, int Bullet_speed, Image image, object_type type) : base(image, type)
        {
            this.location = new Point(Location.X + shooter_size.Width / 2 - this.size.Width / 2, Location.Y);
            this.my_speed = Bullet_speed;
        }

        public override void Update(List<Game_object> everything)
        {
            if(this.location.Y > base.size.Height)
            {
                this.location.Y -= my_speed;
            }
            else
            {
                base.Dispose(everything);
            }
        }
    }
}
