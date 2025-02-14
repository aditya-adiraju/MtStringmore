using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Interface for a developer command in the Dev Console.
    /// </summary>
    public interface IDevCommand
    {
        /// <summary>
        /// Whether this command requires cheats to be enabled.
        /// </summary>
        bool RequiresCheats => true;

        /// <summary>
        /// Command name (i.e. name [args]).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the command given arguments, writing any log messages to the StringWriter.
        /// </summary>
        /// <param name="args">Command arguments</param>
        /// <param name="sw">StringWriter for log messages</param>
        void Run(string[] args, StringWriter sw);

        /// <summary>
        /// Prints all valid usages to the StringWriter.
        /// </summary>
        /// <param name="sw">StringWriter for log messages</param>
        /// <param name="color">Color of printed messages</param>
        void PrintUsage(StringWriter sw, string color = "red");

        /// <summary>
        /// Parses an individual positional argument, which can be relational.
        ///
        /// For instance, ~+20 is player position + 20, but ~20 is also accepted. However, 20~ is not.
        ///
        /// Upon encountering an error, writes out to the StringWriter and returns false.
        /// </summary>
        /// <param name="arg">String argument to parse</param>
        /// <param name="playerCoord">Player coordinate if the argument is relational</param>
        /// <param name="sw">StringWriter to output errors to</param>
        /// <param name="pos">Output position</param>
        /// <returns>Whether parsing was successful: false if not</returns>
        protected static bool TryParsePositionalArgument(string arg, float playerCoord, StringWriter sw, out float pos)
        {
            if (!arg.StartsWith('~'))
            {
                // position is absolute
                if (float.TryParse(arg, out pos)) return true;
                sw.WriteLine(Color($"Invalid positional argument: {arg}", "red"));
                return false;
            }

            arg = arg.Remove(0, 1);
            if (!float.TryParse(arg, out float rel) && arg.Length != 0)
            {
                sw.WriteLine(Color($"Invalid positional argument: {arg}", "red"));
                pos = 0;
                return false;
            }

            pos = rel + playerCoord;
            return true;
        }

        /// <summary>
        /// Parses an X/Y positional argument pair which can be relational.
        ///
        /// For instance, ~+20 is player position + 20, but ~20 is also accepted. However, 20~ is not.
        ///
        /// Unlike <see cref="TryParsePositionalArgument"/>,
        /// this method retrieves the player coordinates rather than taking it as an input.
        /// 
        /// Upon encountering an error, writes out to the StringWriter and returns false.
        /// </summary>
        /// <param name="xarg">X argument</param>
        /// <param name="yarg">Y argument</param>
        /// <param name="sw">StringWriter to output errors to</param>
        /// <param name="pos">Output position</param>
        /// <returns>Whether parsing was successful: false if not</returns>
        /// <seealso cref="TryParsePositionalArgument"/>
        protected static bool TryParsePosition(string xarg, string yarg, StringWriter sw, out Vector2 pos)
        {
            pos = default;
            // if position is absolute, player pos isn't read, so safe to set to 0
            Vector3 playerPos = Vector3.zero;
            if (xarg.StartsWith('~') || yarg.StartsWith('~'))
            {
                // one of the positions is relative, we have to read it
                PlayerController pc = Object.FindObjectOfType<PlayerController>();
                if (!pc)
                {
                    sw.WriteLine(Color("Player not found when requesting relative position.", "red"));
                    return false;
                }

                playerPos = pc.transform.position;
            }

            if (!TryParsePositionalArgument(xarg, playerPos.x, sw, out float x) ||
                !TryParsePositionalArgument(yarg, playerPos.y, sw, out float y)) return false;

            pos = new Vector2(x, y);
            return true;
        }

        /// <summary>
        /// Utility to parse a checkpoint number argument and return the checkpoint's position after checking validity.
        /// </summary>
        /// <param name="checkpointArg">Checkpoint number argument</param>
        /// <param name="sw">StringWriter to output log messages to</param>
        /// <param name="pos">Output position if provided</param>
        /// <returns>Rersult of parsing: true if successful, false otherwise</returns>
        protected static bool TryParseCheckpointPosition(string checkpointArg, StringWriter sw, out Vector2 pos)
        {
            pos = default;
            if (!int.TryParse(checkpointArg, out int checkpointNo))
            {
                sw.WriteLine(Color($"Invalid checkpoint number: {checkpointArg}", "red"));
                return false;
            }

            Checkpoint[] checkpoints = Object.FindObjectsOfType<Checkpoint>();
            if (checkpointNo < 0 || checkpointNo >= checkpoints.Length)
            {
                sw.WriteLine(Color(checkpoints.Length == 0
                    ? "No checkpoints in the scene available to teleport."
                    : $"Invalid checkpoint number: must be in range [0,{checkpoints.Length})", "red"));
                return false;
            }

            pos = checkpoints[checkpointNo].transform.position;
            return true;
        }

        /// <summary>
        /// Utility to parse position arguments that's either (1) checkpoint num or (2) x/y pair.
        ///
        /// X/Y pair can be relational, e.g. ~+20 is player position +20. ~20 is also accepted, but 20~ is not.
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="sw">StringWriter to output log messages to</param>
        /// <param name="pos">Output position if provided</param>
        /// <returns>Result of parsing: true if successful, false otherwise</returns>
        protected static bool TryGetPosOrCheckpointPos(string[] args, StringWriter sw, out Vector2 pos)
        {
            pos = default;
            return args.Length switch
            {
                1 => TryParseCheckpointPosition(args[0], sw, out pos),
                2 => TryParsePosition(args[0], args[1], sw, out pos),
                _ => false
            };
        }

        /// <summary>
        /// Parses 1 or 0 as true/false.
        /// </summary>
        /// <param name="arg">Argument</param>
        /// <param name="result">Parsed result</param>
        /// <returns>True if parsed successfully, false otherwise</returns>
        /// <remarks>
        /// And yes, <see cref="System.Boolean.TryParse(string, out bool)"/> only matches "true" and "false".
        /// </remarks>
        protected static bool TryParseBool(string arg, out bool result)
        {
            result = arg == "1";
            return result || arg == "0";
        }

        /// <summary>
        /// Utility function to color rich text.
        /// </summary>
        /// <param name="text">Text to color</param>
        /// <param name="color">Desired color</param>
        /// <returns>Rich text output with correct color set.</returns>
        public static string Color(string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }
    }
}