using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;

public class ScreamingInjuredPeds : Script
{
    private List<Ped> injured = new List<Ped>();
    private List<Ped> injuredRmv = new List<Ped>();
    private Random r = new Random();

    public ScreamingInjuredPeds()
    {
        Tick += OnTick;
        Interval = 100;
    }

    void OnTick(object sender, EventArgs e)
    {
        foreach (Ped p in World.GetAllPeds()) { //analyze ALL the peds and add them to the Injured list
            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_HURT, p) && p.IsAlive && !injured.Contains(p)) {
                injured.Add(p);
            }
        }

        foreach (Ped p in injured) { //analyze peds in Injured list
            if (p.IsDead) { //if ped is dead, remove it
                injuredRmv.Add(p);
            } else { //if ped is still alive: apply the "injured" screaming sfx
                string faciallib;
                if (p.Gender == Gender.Female) { //ped is female
                    faciallib = "facials@gen_female@base";
                } else { //ped is male
                    faciallib = "facials@gen_male@base";
                }
               /* PAIN STYLES:
                * switchr=1/2 : burning (19)
                * switchr=3 : coughing (8)
                * switchr=other values : don't apply
                */
                int switchr = r.Next(0, 4);
                if (switchr == 3) { //coughing pain sfx
                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, p, 19, 1.0f, 0);
                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_FACIAL_ANIM, p, "coughing_1", faciallib);
                } else if (switchr == 1 || switchr == 2) { //burning pain sfx
                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, p, 8, 1.0f, 0);
                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_FACIAL_ANIM, p, "burning_1", faciallib);
                }
                if (!p.CurrentBlip.Exists()) {
                    p.AddBlip();
                    p.CurrentBlip.Sprite = BlipSprite.TeamDeathmatch;
                    p.CurrentBlip.Color = BlipColor.Yellow;
                    p.CurrentBlip.Name = "Laying injured ped";
                }
            }
        }

        foreach (Ped p in injuredRmv) { //process the remove queue
            injured.Remove(p);
            GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_PED_PAIN_AUDIO, p, true);
            p.CurrentBlip.Remove();
        }
        injuredRmv.Clear();

    }
}