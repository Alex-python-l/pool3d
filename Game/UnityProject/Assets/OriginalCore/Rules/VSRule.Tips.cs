// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
public partial class VSRule
{
    // RVA: 0x92A7C0. All literals resolved through native PC/GOT references to ScriptString.
    public void OnTip(int tipId, int playId)
    {
        if (tipId < 0) return;
        string text = "";
        string name = Pool3D.Offline.OfflineMatchController.Instance.PlayerDisplayName(playId);
        bool replace = false;
        switch (tipId)
        {
            case 0:
                if (name == "You") text = "You have the ball in hand";
                else if (name == Dsp.PLAY_OPPO) text = "Oppo has the ball in hand";
                else { text = "XXX has the ball in hand"; replace = true; }
                break;
            case 1:
                if (name == "You") text = "You are breaking";
                else { text = "XXX's breaking"; replace = true; }
                break;
            case 2: text = "XXX made an illegal break"; replace = true; break;
            case 3: text = "The game was restarted due to the illegal break"; replace = true; break;
            case 4: text = "XXX failed to hit a striped ball"; replace = true; break;
            case 5: text = "XXX failed to hit a solid-colored ball"; replace = true; break;
            case 6: text = "XXX potted the cue ball"; replace = true; break;
            case 7: text = "The cue ball did not strike another ball"; replace = true; break;
            case 8: text = "Illegal break"; replace = true; break;
            case 9: text = "Time is out"; break;
            case 100:
                if (name == "You") text = "Your turn";
                else { text = "XXX's turn"; replace = true; }
                break;
            case 101: text = "XXX give up the turn"; replace = true; break;
            case 102: text = "XXX push-out!"; replace = true; break;
        }
        if (replace) text = text.Replace("XXX", name);
        if (m_Listener != null) m_Listener.Tip(text);
    }
}
