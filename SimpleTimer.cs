namespace LD56;

public class SimpleTimer
{
    private readonly double interval;
    private readonly bool immediate;
    private double timeSinceLast;

    public SimpleTimer(double interval, bool immediate = true)
    {
        this.interval = interval;
        this.immediate = immediate;
        Reset();
    }

    public void Reset()
    {
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