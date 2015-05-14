using ICities;

namespace EnhancedAmbulanceAI
{
    public class Identity : IUserMod
    {
        public string Name
        {
            get { return Settings.Instance.Tag; }
        }

        public string Description
        {
            get { return "Oversees emergency services to ensure that ambulance trucks are dispatched in an efficient manner."; }
        }
    }
}