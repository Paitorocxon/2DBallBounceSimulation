using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class Ball
{
    public PointF Position { get; set; }
    public PointF Velocity { get; set; }
    public float Radius { get; set; }
    public Color Color { get; set; }

    public Ball(PointF position, PointF velocity, float radius, Color color)
    {
        Position = position;
        Velocity = velocity;
        Radius = radius;
        Color = color;
    }

    public void Update(float deltaTime, RectangleF bounds, List<Ball> balls, float gravity, float repulsionForce)
    {
        // Update ball position based on velocity
        PointF displacement = new PointF(Velocity.X * deltaTime, Velocity.Y * deltaTime);
        Position = new PointF(Position.X + displacement.X, Position.Y + displacement.Y);

        // Apply gravity
        Velocity = new PointF(Velocity.X, Velocity.Y + gravity * deltaTime);

        // Check for collisions with the boundaries
        if (Position.X - Radius < bounds.Left || Position.X + Radius > bounds.Right)
        {
            Velocity = new PointF(-Velocity.X, Velocity.Y); // Bounce off the vertical walls
        }
        if (Position.Y - Radius < bounds.Top || Position.Y + Radius > bounds.Bottom)
        {
            Velocity = new PointF(Velocity.X, -Velocity.Y); // Bounce off the horizontal walls
        }

        // Check for collisions with other balls
        foreach (var ball in balls)
        {
            if (ball != this)
            {
                float distance = CalculateDistance(Position, ball.Position);
                float minDistance = Radius + ball.Radius;

                if (distance < minDistance)
                {
                    // Calculate the direction vector between the balls
                    PointF direction = new PointF(ball.Position.X - Position.X, ball.Position.Y - Position.Y);
                    direction = Normalize(direction);

                    // Calculate the repulsion force
                    float force = repulsionForce / distance;

                    // Update velocities of both balls
                    Velocity = new PointF(Velocity.X - force * direction.X, Velocity.Y - force * direction.Y);
                    ball.Velocity = new PointF(ball.Velocity.X + force * direction.X, ball.Velocity.Y + force * direction.Y);
                }
            }
        }
    }

    public void Draw(Graphics graphics)
    {
        float diameter = Radius * 2;
        RectangleF bounds = new RectangleF(Position.X - Radius, Position.Y - Radius, diameter, diameter);

        // Create a path for the ball
        GraphicsPath path = new GraphicsPath();
        path.AddEllipse(bounds);

        // Create a path gradient brush
        using (PathGradientBrush brush = new PathGradientBrush(path))
        {
            // Set the center color to the ball's color
            brush.CenterColor = Color;

            // Set the surrounding colors to a lighter shade of the ball's color
            Color[] surroundingColors = { Color.FromArgb(200, Color), Color.FromArgb(100, Color) };
            brush.SurroundColors = surroundingColors;

            // Fill the ellipse with the gradient brush
            graphics.FillEllipse(brush, bounds);
        }
    }

    private float CalculateDistance(PointF point1, PointF point2)
    {
        float dx = point2.X - point1.X;
        float dy = point2.Y - point1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private PointF Normalize(PointF vector)
    {
        float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        return new PointF(vector.X / length, vector.Y / length);
    }
}

public class Form1 : Form
{
    private Timer timer;
    private Random random;
    private List<Ball> balls;

    public Form1()
    {
        this.ClientSize = new Size(800, 600);
        this.DoubleBuffered = true;

        random = new Random();
        balls = new List<Ball>();
        for (int i = 0; i < 10; i++)
        {
            PointF position = new PointF(random.Next(50, Width - 50), random.Next(50, Height - 50));
            PointF velocity = new PointF((float)(random.NextDouble() * 200 - 100), (float)(random.NextDouble() * 200 - 100));
            float radius = random.Next(10, 50);
            Color color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            balls.Add(new Ball(position, velocity, radius, color));
        }

        timer = new Timer();
        timer.Interval = 16; // 16 milliseconds (approximately 60 FPS)
        timer.Tick += new EventHandler(UpdateBalls);
        timer.Start();
    }

    private void UpdateBalls(object sender, EventArgs e)
    {
        float deltaTime = timer.Interval / 1000f;

        foreach (var ball in balls)
        {
            ball.Update(deltaTime, this.ClientRectangle, balls, 980f, 10000f);
        }

        this.Invalidate(); // Trigger redraw
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;

        foreach (var ball in balls)
        {
            ball.Draw(graphics);
        }
    }

}
