namespace Org.Edgerunner.Moo.Editor;

public static class Moo
{
   static Moo()
   {
      foreach (var builtin in BuiltinsWithArgs)
         Builtins[builtin] = builtin + "(^)";
      foreach (var builtin in BuiltinsNoArgs)
         Builtins[builtin] = builtin + "()^";
   }

   public static Dictionary<string, string> Builtins = new(200);

   public static IList<string> Keywords = new List<string>
   {
      "if", "elseif", "else", "endif", "return", "while", "endwhile", "for", "endfor", "fork", "endfork", "try", "except", "finally",
      "endtry", "in", "player", "caller", "verb", "dobj", "dobjstr", "prepstr", "iobj", "iobjstr", "this", "args", "argstr",
      "E_NONE", "E_TYPE", "E_DIV", "E_PERM", "E_PROPNF", "E_VERBNF", "E_VARNF", "E_INVIND", "E_RECMOVE", "E_MAXREC", "E_RANGE", "E_ARGS", "E_NACC", "E_INVARG", "E_QUOTA",
      "E_FLOAT", "E_FILE", "E_EXEC", "E_INTRPT", "STR", "LIST", "OBJ", "MAP", "INT", "FLOAT", "ERR", "BOOL", "WAIF", "ANON", "NUM", "true", "false"
   };

   private static readonly List<string> BuiltinsWithArgs = new()
   {
      "typeof", "tostr", "toint", "tofloat", "toobj", "toliteral", "typeof", "equal", "string_hash", "binary_hash", "value_bytes", "value_hash", "value_hmac",
      "generate_json", "parse_json", "random", "frandom", "random_bytes", "reseed_random", "min", "max", "abs", "exp", "floatstr", "sqrt", "sin", "cos",
      "tan", "asin", "acos", "atan", "sinh", "cosh", "tanh", "log", "log10", "ceil", "floor", "trunc", "length", "strsub", "index", "rindex", "strstr",
      "strcmp", "explode", "decode_binary", "encode_binary", "decode_base64", "encode_base64", "spellcheck", "chr", "match", "rmatch", "pcre_match", "pcre_replace",
      "substitute", "salt", "crypt", "argon2", "argon2_verify", "string_hmac", "binary_hmac", "is_member", "all_members", "listinsert", "listappend", "listdelete",
      "listset", "setadd", "setremove", "reverse", "slice", "sort", "mapkeys", "mapvalues", "mapdelete", "maphaskey", "create", "owned_objects", "chparent", "chparents",
      "valid", "parent", "parents", "isa", "children", "locate_by_name", "locations", "occupants", "recycle", "recreate", "next_recycled_object", "recycled_objects",
      "ancestors", "clear_ancestor_cache", "descendants", "object_bytes", "respond_to", "move", "properties", "property_info", "set_property_info", "add_property",
      "delete_property", "is_clear_property", "clear_property", "verbs", "verb_info", "set_verb_info", "verb_args", "set_verb_args", "add_verb", "delete_verb",
      "verb_code", "set_verb_code", "disassemble", "new_waif", "is_player", "set_player_flag", "boot_player", "file_open", "file_close", "file_name", "file_openmode",
      "file_readline", "file_readlines", "file_writeline", "file_read", "file_write", "file_count_lines", "file_grep", "file_tell", "file_seek", "file_eof", "file_stat",
      "file_size", "file_last_access", "file_last_modify", "file_last_change", "file_modify", "file_change", "file_mode", "file_rename", "file_remove", "file_mkdir",
      "file_rmdir", "file_list", "file_type", "file_chmod", "sqlite_open", "sqlite_close", "sqlite_execute", "sqlite_query", "sqlite_limit", "sqlite_last_insert_row_id",
      "sqlite_interrupt", "sqlite_info", "exec", "getenv", "connected_players", "connected_seconds", "notify", "read", "buffered_output_length", "set_connection_option",
      "force_input", "flush_input", "output_delimiters", "connection_info", "connection_name", "connection_name_lookup", "set_connection_option", "connection_options",
      "switch_player", "open_network_connection", "curl", "read_http", "listen", "unlisten", "ftime", "ctime", "raise", "call_function", "function_info", "eval",
      "set_task_perms", "set_thread_mode", "thread_info", "thread_pool", "suspend", "resume", "yin", "queue_info", "queued_tasks", "kill_task", "callers", "task_stack",
      "server_version", "server_log", "renumber", "panic", "shutdown"
   };

   private static readonly List<string> BuiltinsNoArgs = new()
   {
      "caller_perms", "max_object", "waif_stats", "players", "file_version", "file_handles", "sqlite_handles", "listeners", "time", "task_local", "threads", "ticks_left",
      "seconds_left", "task_id", "finished_tasks", "reset_max_object", "memory_usage", "usage", "dump_database", "db_disk_size", "log_cache_stats"
   };
}