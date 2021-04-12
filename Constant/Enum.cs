namespace ChatAPI.Constant
{
    public enum PublishType
    {
        Private = 1,
        MemberAdd = 2,
        Publish = 3
    }

    public enum GroupType
    {
        Single = 2,
        Group = 1
    }

    public enum Approved
    {
        CreateNew = 0,
        Approved = 1,
        Reject = 2
    }

    public enum SocketTask
    {
        // Login
        Login = 1,

        // action group chat
        CreateGroup = 11,
        CreateChat = 12,
        AddToGroup = 21,
        JoinToGroup = 22,
        RemoveFromGroup = 31,
        LeaveFromGroup = 32,
        //SearchingGroup = 40,

        //// action User chat
        //AddReportUser = 50,
        //AddBlacklistUser = 61,
        //RemoveBlacklistUser = 62,

        // action message
        MessageEdit = 74,
        MessageSend = 70,
        MessageRecall = 71,
        MessageRead = 72,
        MessageDelete = 73
    }

    public enum MessageType
    {
        Text = 1,
        Image = 2,
        Video = 3,
        Streaming = 4,
        Excel = 5,
        Word = 6,
        Pdf = 7,
        File = 9
    }
}
