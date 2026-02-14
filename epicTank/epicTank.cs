using System;
using System.Threading;
using Robocode.TankRoyale.BotApi.Graphics;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;


public class epicTank : Bot
{
    // trigger threshold for custom event
    internal double trigger = 20;
    // movement and radar state
    private int moveDirection = 1; // 1 = forward, -1 = back
    private int radarDirection = 1; // 1 = right, -1 = left
    private double moveDistance = 1000; // distance to move on each command
    // The main method starts our bot
    static void Main(string[] args)
    {
        new epicTank().Start();
    }


    public override void Run()
    {

        // movement and radar logic goes here
        while (IsRunning)
        {
            // --- Radar: continuously sweep in one direction ---
            // Turn the radar a modest step; reversing is handled in OnScannedBot
            SetTurnRadarLeft(double.PositiveInfinity);

            // Set colors
            BodyColor = Color.Green;
            TurretColor = Color.Green;
            RadarColor = Color.Green;

            // Add a custom event named "trigger-hit"
            AddCustomEvent(new TriggerHit(this));

            // Movement strategy: move forward (or back) until hit wall, then reverse
            if (moveDirection == 1)
            {
                Forward(moveDistance);
            }
            else
            {
                Back(moveDistance);
            }
            Execute();
        }
    }

    // We collided with a wall -> reverse the direction
    public override void OnHitWall(HitWallEvent e)
    {
        // Bounce off: reverse movement direction and continue
        moveDirection *= -1;
        if (moveDirection == 1) Forward(moveDistance);
        else Back(moveDistance);
    }
    

    public override void OnCustomEvent(CustomEvent evt)
    {
        // Check if our custom event "trigger-hit" went off
        if (evt.Condition.Name == "trigger-hit")
        {
            // Adjust the trigger value, or else the event will fire again and again and again...
            trigger -= 1;

            // Print out energy level
            Console.WriteLine("Ouch, down to " + (int)(Energy + .5) + " energy.");

            // Move around a bit
            TurnLeft(90);
        }
    }

private void SmartFire(double distance)
{
    if (distance > 200 || Energy < 15)
    {
            Fire(1);
    }
        else if (distance < 100)
    {
            Fire(2);
    }
}

// firing and radar logic goes here
public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Radar strategy: reverse radar direction to lock on scanned target
        radarDirection *= -1;
        TurnRadarRight(360 * radarDirection);

        // Firing logic
        var distance = DistanceTo(evt.X, evt.Y);
        SmartFire(distance);
    }

// We were hit by a bullet -> turn a random direction and go
public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Calculate the bearing to the direction of the bullet
        var bearing = CalcBearing(evt.Bullet.Direction);

        // Turn a (mostly) perpendicular plus a small random offset, then change movement
        var offset = Random.Shared.NextDouble() * 90;
        TurnRight(90 - bearing + offset);

        // change movement direction and move
        moveDirection *= -1;
        if (moveDirection == 1) Forward(moveDistance);
        else Back(moveDistance);
    }
}
class TriggerHit : Condition
{
    private epicTank bot;

    internal TriggerHit(epicTank bot) : base("trigger-hit")
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        return bot.Energy <= bot.trigger;
    }
}
