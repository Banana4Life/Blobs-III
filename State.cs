namespace LD56;

public interface State
{
    void Update(double dt);
    void PlayerDisconnected(int peerId);
}