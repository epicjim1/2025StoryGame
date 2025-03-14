// Character relationships
VAR johnny_missing = true
VAR carl_spoken_to = false
VAR carl_dead = false
VAR maximus_helping = false
VAR captain_rex_helping = false
VAR knights_hostile = false

-> JANE_DOE

=== start ===
You wake up as John Doe and find that your son, Johnny Doe, is gone.  
-> JANE_DOE

// Asking Jane Doe
=== JANE_DOE ===
You finally woke up, Johnny didnt come home yet! He said he would be back by morning but its already noon.
+ {johnny_missing} [I am sure he's fine. Where was he last night?] He was out with Carl all day and slept at his house slept at Carl's. Johnny said he would be back by the morning
    Where does Carl live again?
    the house with the red door. -> DONE
//+++ [Ok, I will bring him home] -> DONE


// Speaking to Carl
=== CARL ===
~ carl_spoken_to = true
NPC: You find Carl sitting near the market. He looks nervous.  
Player: You look like you’ve seen a ghost,  

+ ["Where is Johnny?"] NPC: Carl sighs. "He… he joined a gang. They needed an extra hand for a robbery."
    -> carl_explains

=== carl_explains ===
Carl looks around before whispering.  
"The robbery went wrong. Everyone ran when the royal knights arrived. I don’t know where Johnny went, but I swear I didn't mean to get him involved!"  

+ ["So you're telling me my son is missing?"] "Yes… but I can help you find him!" Carl insists.
    -> encounter_knights

+ ["You’re a coward! You left him behind!"] Carl flinches. "I know! I know! I screwed up, okay?"
    -> encounter_knights

=== decide_next_step ===
Now you need to find Johnny. There are two ways:

+ #sticky [Ask Maximus for help] -> talk_to_maximus 
+ #sticky [Seek help from Captain Rex] -> talk_to_captain_rex

// Encounter with the Royal Knights
=== encounter_knights ===
As you walk away, two royal knights stop you.  
"Why were you speaking with Carl?" one demands.  
"We know he took part in the robbery. He is a criminal, and we are here to kill him."  

+ #sticky [Let them kill Carl] 
    ~ carl_dead = true
    Carl’s eyes widen as the knights cut him down.  
    "Justice is served," one knight says before leaving.  
    -> seek_captain_rex_or_maximus

+ #sticky [Defend Carl] 
    ~ knights_hostile = true
    You step between Carl and the knights.  
    "He's just a kid! He didn't plan the robbery!" you argue.  
    "That makes you a criminal, too," the knight sneers.  
    -> fight_knights

=== fight_knights ===
The knights draw their swords. You have no choice but to fight.  
(Combat happens in Unity)  
-> seek_captain_rex_or_maximus

// Seeking Help
=== seek_captain_rex_or_maximus ===
You need allies. Who do you approach?

+ [Captain Rex] -> talk_to_captain_rex
+ [Maximus] -> talk_to_maximus

// Talking to Captain Rex
=== talk_to_captain_rex ===
"You say the gang took your son? And you expect me to let a peasant hunt criminals?"  
Captain Rex crosses his arms.  

+ ["I can handle myself."]
    "Prove it. Kill three trolls, and I might help you."
    -> prove_yourself_to_rex

+ ["I just need information!"]
    "Information isn’t free. Earn it."
    -> prove_yourself_to_rex

// Proving Yourself to Captain Rex
=== prove_yourself_to_rex ===
{captain_rex_helping == false:
    "If you want my help, complete a task for me first," Captain Rex commands.
}
-> END

// Talking to Maximus
=== talk_to_maximus ===
You meet Maximus in the alley behind the tavern.  
"I heard about Johnny. The gang's been lurking near the forest."  

+ ["Will you help me find them?"]
    "I could… if I get something in return."  
    -> END
