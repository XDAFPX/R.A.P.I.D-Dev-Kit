using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    public class HitBoxOfEntity : HitBox<IEntity>
    {
        protected override IEnumerable<IEntity> BuildContext(IEnumerable<HurtBox<IEntity>> hits)
        {
            var _l = new List<IEntity>();
            var _data = hits.Select((box => box.GetCtx()));
            foreach (var _hurtBox in _data)
            {
                if(_l.Contains(_hurtBox.Ctx))
                    continue;
                _hurtBox.Data.FlaggedBox.Hurt(this, ((IOwnedBy<IEntity>)this).GetCurrentOwner()  );
                _l.Add(_hurtBox.Ctx);
            }

            return _l;
        }
    }
}