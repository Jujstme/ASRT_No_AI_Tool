namespace ASRT_No_AI;

internal static class Messages
{
    const string GAME_NAME = "ASRT No AI Tool";

    internal static void GameNotFoundError()
    {
        MessageBox.Show("Please start the game first!", GAME_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    internal static void VersionMismatchError()
    {
        MessageBox.Show("Cannot apply patch. Please ensure you are\n" +
            "running the correct version of the game!", GAME_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    internal static void CannotAccessMemoryError()
    {
        MessageBox.Show("An unexpected error occurred while trying to access the memory of the game process.\n" +
            "Ensure your user has enough permissions to do so.\n\n" +
            "Check if the game is running under admin privileges.\n" +
            "If so, this tool needs to be run as admin as well!", GAME_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    internal static void Success()
    {
        MessageBox.Show("You are now free from the AI in Single Race mode!", GAME_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    internal static DialogResult GameAlreadyPatched()
    {
        return MessageBox.Show("It appears your game is already patched\n" +
            "Do you want to remove the patch and restore stock settings?", GAME_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }

    internal static void GameUnpatched()
    {
        MessageBox.Show("No AI mod has been disabled!\n" +
            "Enjoy your stock experience!", GAME_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
