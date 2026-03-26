using Microsoft.AspNetCore.SignalR;
using PRN232_BE.Models;

namespace PRN232_BE.Hubs;

public class ChatHub : Hub
{
    private readonly CloneEbayDb1Context _context;

    public ChatHub(CloneEbayDb1Context context)
    {
        _context = context;
    }

    public async Task JoinRoom(int roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroupName(roomId));
    }

    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroupName(roomId));
    }

    public async Task SendMessageToRoom(int roomId, int senderId, string content)
    {
        var msg = new Message
        {
            RoomId = roomId,
            SenderId = senderId,
            Content = content,
            IsRead = false,
            Date = DateTime.UtcNow
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        await Clients.Group(RoomGroupName(roomId)).SendAsync("ReceiveMessage", new
        {
            Id = msg.Id,
            RoomId = msg.RoomId,
            SenderId = msg.SenderId,
            Content = msg.Content,
            IsRead = msg.IsRead,
            Date = msg.Date
        });
    }

    private static string RoomGroupName(int roomId) => $"room-{roomId}";
}