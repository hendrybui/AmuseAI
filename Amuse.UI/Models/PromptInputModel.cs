namespace Amuse.UI.Models
{
    public record PromptInputModel
    {
        public PromptInputModel(string prompt, PromptInputType type)
        {
            Prompt = prompt;
            Type = type;
        }

        public string Prompt { get; set; }
        public PromptInputType Type { get; set; }
    }

    public enum PromptInputType
    {
        Positive = 0,
        Negative = 1,
        Snippit = 3
    }
}
