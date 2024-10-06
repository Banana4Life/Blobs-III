namespace LD56;

public class SimpleTimer
{
    private readonly double interval;
    private double timeSinceLast;

    public SimpleTimer(double interval, bool immediate = true)
    {
        this.interval = interval;
        timeSinceLast = immediate ? interval : 0d;
    }

    public bool Update(double dt)
    {
        timeSinceLast += dt;
        if (timeSinceLast >= interval)
        {
            timeSinceLast -= interval;
            return true;
        }

        return false;
    }
}