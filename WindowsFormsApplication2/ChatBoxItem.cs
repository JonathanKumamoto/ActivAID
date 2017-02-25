using System;


public class ChatBoxItem
{
    public ChatBoxItem(Color c, string m)
    {
        ItemColor = c;
        Message = m;
    }
    public Color ItemColor { get; set; }
    public string Message { get; set; }
}