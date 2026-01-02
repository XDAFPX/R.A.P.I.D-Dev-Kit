namespace BandoWare.GameplayTags
{
    public abstract class TagBuilder<T> where T : TagBuilder<T>

    {
        protected readonly GameplayTagContainer Container = new GameplayTagContainer();

        public virtual T RemoveTag(GameplayTag tag)
        {
            Container.RemoveTag(tag);
            return (T)this;
        }

        public virtual T AddTag(GameplayTag tag)
        {
            Container.AddTag(tag);
            return (T)this;
        }

        public virtual GameplayTagContainer Build()
        {
            return Container;
        }
    }
}