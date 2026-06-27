@prop #XXX.use_generate_json 0
@prop #XXX.version_range {}
@prop #XXX.messages_in {}
@prop #XXX.messages_out {}
@prop #XXX.aliases {}
@prop #XXX.description {}

;;#XXX.("use_generate_json") = -1
;;#XXX.("version_range") = {"1.0", "1.0"}
;;#XXX.("messages_in") = {{"core-objects", {"tag"}}, {"player", {"tag"}}, {"children", {"tag", "object"}}, {"owned", {"tag", "owner"}}, {"parent", {"tag", "object"}}, {"verbs", {"tag", "object"}}, {"verb-info", {"tag", "object", "verb"}}, {"verb-doc", {"tag", "object", "verb"}}, {"verb-code", {"tag", "object", "verb"}}, {"props", {"tag", "object"}}, {"prop-info", {"tag", "object", "prop"}}, {"prop-doc", {"tag", "object", "prop"}}, {"prop-value", {"tag", "object", "prop"}}, {"constant-value", {"tag", "constant"}}, {"constant-tostr", {"tag", "constant"}}}
;;#XXX.("messages_out") = {{"core-objects-reply", {"tag", "data"}}, {"player-reply", {"tag", "data"}}, {"children-reply", {"tag", "data"}}, {"owned-reply", {"tag", "data"}}, {"parent-reply", {"tag", "data"}}, {"verbs-reply", {"tag", "data"}}, {"verb-info-reply", {"tag", "data"}}, {"verb-doc-reply", {"tag", "data"}}, {"verb-code-reply", {"tag", "data"}}, {"props-reply", {"tag", "data"}}, {"prop-info-reply", {"tag", "data"}}, {"prop-doc-reply", {"tag", "data"}}, {"prop-value-reply", {"tag", "data"}}, {"constant-value-reply", {"tag", "data"}}, {"constant-tostr-reply", {"tag", "data"}}, {"error", {"tag", "code", "message"}}}
;;#XXX.("aliases") = {"edgerunner-org-moo-query"}
;;#XXX.("description") = {"Developer-information query package for MCP 2.1 clients (v1.0).", "", "Each C->S request carries a client-generated tag; each S->C reply echoes the", "tag and carries one data* multiline field holding minified JSON.", "Object numbers are bare JSON ints; verb names are raw MOO names strings.", "", "Requests (params besides tag):", " -core-objects ()            -> {\"d\":[[num,name,[aliases]],...]}", " -player ()                  -> {\"p\":num}  connected player object", " -children (object)          -> {\"d\":[[num,name,[aliases]],...]}", " -owned (owner)              -> {\"d\":[[num,name,[aliases]],...]}  owner \"\" = player", " -parent (object)            -> {\"p\":num}  -1 = no parent", " -verbs (object)             -> {\"d\":[[\"g*et put\",isLocal],...]}  isLocal 1=local 0=inherited, deduped", " -verb-info (object, verb)   -> {\"q\",\"r\",\"a\",\"o\",\"p\",\"g\"}", " -verb-doc (object, verb)    -> {\"q\",\"r\",\"l\":[lines]}", " -verb-code (object, verb)   -> {\"q\",\"r\",\"l\":[lines]}", " -props (object)             -> {\"d\":[[\"name\",isLocal],...]}  isLocal 1=local 0=inherited, deduped", " -prop-info (object, prop)   -> {\"n\",\"o\",\"p\",\"t\",\"v\"}  v = 80-char preview", " -prop-doc (object, prop)    -> {\"l\":[lines]}  toliteral split <=78 chars, max 50", " -prop-value (object, prop)  -> {\"t\",\"v\"}  full toliteral", " -constant-value (constant)  -> {\"v\":\"<toliteral>\"}  eval'd value of a bare-identifier constant", " -constant-tostr (constant)  -> {\"v\":\"<tostr>\"}  tostr() of a bare-identifier constant", "", "Shared error reply: -error (tag, code, message) where code is the MOO error", "name (E_PERM, E_INVARG, E_VERBNF, E_PROPNF, ...).", "", "Every handler runs under set_task_perms() of the connected player; normal MOO", "read rules decide visibility.", "", "Normative protocol: docs/edgerunner-org-moo-query-protocol.md in the", "Moo Developer Tools repository (https://github.com/.../Moo-Developer-Tools)."}

@verb #XXX:"handle_core-objects" this none this rxd
@verb #XXX:"handle_player" this none this rxd
@verb #XXX:"handle_children" this none this rxd
@verb #XXX:"handle_owned" this none this rxd
@verb #XXX:"handle_parent" this none this rxd
@verb #XXX:"handle_verbs" this none this rxd
@verb #XXX:"handle_verb-info" this none this rxd
@verb #XXX:"handle_verb-doc" this none this rxd
@verb #XXX:"handle_verb-code" this none this rxd
@verb #XXX:"handle_props" this none this rxd
@verb #XXX:"handle_prop-info" this none this rxd
@verb #XXX:"handle_prop-doc" this none this rxd
@verb #XXX:"handle_prop-value" this none this rxd
@verb #XXX:"handle_constant-value" this none this rxd
@verb #XXX:"handle_constant-tostr" this none this rxd
@verb #XXX:"valid_constant_name" this none this rxd
@verb #XXX:"find_verb_definer" this none this rxd
@verb #XXX:"summary_json" this none this rxd
@verb #XXX:"json_encode" this none this rxd
@verb #XXX:"send_reply" this none this rxd
@verb #XXX:"send_error" this none this rxd

@args #XXX:"handle_core-objects" this none this
@chown #XXX:handle_core-objects #2
@program #XXX:handle_core-objects
"Usage: :handle_core-objects(session, tag)";
{session, tag} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  objs = {};
  for pname in (properties(#0))
    value = `#0.(pname) ! ANY => #-1';
    if (typeof(value) == OBJ && valid(value) && !(value in objs))
      objs = {@objs, value};
    endif
  endfor
  this:send_reply(session, "core-objects-reply", tag, this:summary_json(objs));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_player" this none this
@chown #XXX:handle_player #2
@program #XXX:handle_player
"Usage: :handle_player(session, tag) -- the connected player object for this session";
{session, tag} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  this:send_reply(session, "player-reply", tag, tostr("{\"p\":", toint(session.connection), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_children" this none this
@chown #XXX:handle_children #2
@program #XXX:handle_children
"Usage: :handle_children(session, tag, object)";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  this:send_reply(session, "children-reply", tag, this:summary_json(children(o)));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_owned" this none this
@chown #XXX:handle_owned #2
@program #XXX:handle_owned
"Usage: :handle_owned(session, tag, owner) -- owner \"\" or absent means the connected player";
{session, tag, ?owner = ""} = args;
if (caller != this)
  raise(E_PERM);
endif
me = session.connection;
set_task_perms(me);
try
  if (owner == "" || owner == 0)
    target = me;
  else
    target = toobj(owner);
  endif
  if (typeof(target) != OBJ || !valid(target))
    raise(E_INVARG);
  endif
  owned = `target.owned_objects ! E_PROPNF';
  if (typeof(owned) == ERR)
    raise(E_INVARG, "This core has no owned_objects bookkeeping");
  endif
  if (typeof(owned) != LIST)
    owned = {};
  endif
  objs = {};
  for o in (owned)
    if (typeof(o) == OBJ && valid(o))
      objs = {@objs, o};
    endif
  endfor
  this:send_reply(session, "owned-reply", tag, this:summary_json(objs));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_parent" this none this
@chown #XXX:handle_parent #2
@program #XXX:handle_parent
"Usage: :handle_parent(session, tag, object)";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  this:send_reply(session, "parent-reply", tag, tostr("{\"p\":", toint(parent(o)), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verbs" this none this
@chown #XXX:handle_verbs #2
@program #XXX:handle_verbs
"Usage: :handle_verbs(session, tag, object) -- local + inherited verb-names, deduped";
"Each reply row is {name, isLocal}: isLocal is 1 when the name was found on the queried";
"object itself (the first iteration, what == o) and 0 when inherited from an ancestor.";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  names = {};
  rows = {};
  what = o;
  while (valid(what))
    local = (what == o);
    for vname in (`verbs(what) ! E_PERM => {}')
      "Nearest definition wins, so an overridden name keeps its local isLocal=1 flag.";
      if (!(vname in names))
        names = {@names, vname};
        rows = {@rows, {vname, local ? 1 | 0}};
      endif
    endfor
    what = parent(what);
  endwhile
  this:send_reply(session, "verbs-reply", tag, tostr("{\"d\":", this:json_encode(rows), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb-info" this none this
@chown #XXX:handle_verb-info #2
@program #XXX:handle_verb-info
"Usage: :handle_verb-info(session, tag, object, verb)";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  info = verb_info(r, vname);
  vargs = verb_args(r, vname);
  json = tostr("{\"q\":", toint(o), ",\"r\":", toint(r), ",\"a\":", this:json_encode(info[3]), ",\"o\":", toint(info[1]), ",\"p\":", this:json_encode(info[2]), ",\"g\":", this:json_encode(vargs), "}");
  this:send_reply(session, "verb-info-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb-doc" this none this
@chown #XXX:handle_verb-doc #2
@program #XXX:handle_verb-doc
"Usage: :handle_verb-doc(session, tag, object, verb) -- leading string-literal lines of the code";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  code = verb_code(r, vname);
  docs = {};
  for line in (code)
    mat = match(line, "^ *\"%(.*%)\"; *$");
    if (mat)
      inner = substitute("%1", mat);
      inner = strsub(strsub(inner, "\\\"", "\""), "\\\\", "\\");
      docs = {@docs, inner};
    else
      break;
    endif
  endfor
  json = tostr("{\"q\":", toint(o), ",\"r\":", toint(r), ",\"l\":", this:json_encode(docs), "}");
  this:send_reply(session, "verb-doc-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_verb-code" this none this
@chown #XXX:handle_verb-code #2
@program #XXX:handle_verb-code
"Usage: :handle_verb-code(session, tag, object, verb)";
{session, tag, object, vname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  r = this:find_verb_definer(o, vname);
  lines = verb_code(r, vname);
  json = tostr("{\"q\":", toint(o), ",\"r\":", toint(r), ",\"l\":", this:json_encode(lines), "}");
  this:send_reply(session, "verb-code-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_props" this none this
@chown #XXX:handle_props #2
@program #XXX:handle_props
"Usage: :handle_props(session, tag, object) -- local + inherited property names, deduped";
"Each reply row is {name, isLocal}: isLocal is 1 when the name was found on the queried";
"object itself (the first iteration, what == o) and 0 when inherited from an ancestor.";
{session, tag, object} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  names = {};
  rows = {};
  what = o;
  while (valid(what))
    local = (what == o);
    for pname in (`properties(what) ! E_PERM => {}')
      "Nearest definition wins, so an overridden name keeps its local isLocal=1 flag.";
      if (!(pname in names))
        names = {@names, pname};
        rows = {@rows, {pname, local ? 1 | 0}};
      endif
    endfor
    what = parent(what);
  endwhile
  this:send_reply(session, "props-reply", tag, tostr("{\"d\":", this:json_encode(rows), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop-info" this none this
@chown #XXX:handle_prop-info #2
@program #XXX:handle_prop-info
"Usage: :handle_prop-info(session, tag, object, prop)";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  info = property_info(o, pname);
  value = o.(pname);
  lit = toliteral(value);
  preview = lit[1..min(80, length(lit))];
  json = tostr("{\"n\":", this:json_encode(pname), ",\"o\":", toint(info[1]), ",\"p\":", this:json_encode(info[2]), ",\"t\":", typeof(value), ",\"v\":", this:json_encode(preview), "}");
  this:send_reply(session, "prop-info-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop-doc" this none this
@chown #XXX:handle_prop-doc #2
@program #XXX:handle_prop-doc
"Usage: :handle_prop-doc(session, tag, object, prop) -- toliteral split into <=78-char lines, max 50";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  lit = toliteral(o.(pname));
  lines = {};
  start = 1;
  len = length(lit);
  while (start <= len && length(lines) < 50)
    finish = min(start + 77, len);
    lines = {@lines, lit[start..finish]};
    start = finish + 1;
  endwhile
  this:send_reply(session, "prop-doc-reply", tag, tostr("{\"l\":", this:json_encode(lines), "}"));
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_prop-value" this none this
@chown #XXX:handle_prop-value #2
@program #XXX:handle_prop-value
"Usage: :handle_prop-value(session, tag, object, prop)";
{session, tag, object, pname} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  o = toobj(object);
  if (!valid(o))
    raise(E_INVARG);
  endif
  value = o.(pname);
  json = tostr("{\"t\":", typeof(value), ",\"v\":", this:json_encode(toliteral(value)), "}");
  this:send_reply(session, "prop-value-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_constant-value" this none this
@chown #XXX:handle_constant-value #2
@program #XXX:handle_constant-value
"Usage: :handle_constant-value(session, tag, name)";
"Returns the raw value of a named MOO language constant (type code, error, bool) as toliteral(),";
"e.g. NUM -> {\"v\":\"0\"}. The name is validated to a bare identifier first so eval() can only";
"resolve a single constant token and never run arbitrary code.";
{session, tag, name} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  if (!this:valid_constant_name(name))
    raise(E_INVARG);
  endif
  result = eval(tostr("return ", name, ";"));
  if (!result[1])
    raise(E_INVARG);
  endif
  json = tostr("{\"v\":", this:json_encode(toliteral(result[2])), "}");
  this:send_reply(session, "constant-value-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"handle_constant-tostr" this none this
@chown #XXX:handle_constant-tostr #2
@program #XXX:handle_constant-tostr
"Usage: :handle_constant-tostr(session, tag, name)";
"Returns tostr() of a named MOO language constant, e.g. E_PERM -> {\"v\":\"Permission denied\"}.";
"The name is validated to a bare identifier first; eval() can only resolve a single constant token.";
{session, tag, name} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  if (!this:valid_constant_name(name))
    raise(E_INVARG);
  endif
  result = eval(tostr("return tostr(", name, ");"));
  if (!result[1])
    raise(E_INVARG);
  endif
  json = tostr("{\"v\":", this:json_encode(result[2]), "}");
  this:send_reply(session, "constant-tostr-reply", tag, json);
except v (ANY)
  this:send_error(session, tag, v);
endtry
.

@args #XXX:"valid_constant_name" this none this
@chown #XXX:valid_constant_name #2
@program #XXX:valid_constant_name
"Usage: :valid_constant_name(STR) => 1 when name is a bare identifier (letter/underscore start,";
"then letters/digits/underscore, length 1..40), else 0. Restricts eval() to a single constant token.";
{name} = args;
if (typeof(name) != STR || name == "" || length(name) > 40)
  return 0;
endif
first = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
rest = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
if (!index(first, name[1]))
  return 0;
endif
for i in [1..length(name)]
  if (!index(rest, name[i]))
    return 0;
  endif
endfor
return 1;
.

@args #XXX:"find_verb_definer" this none this
@chown #XXX:find_verb_definer #2
@program #XXX:find_verb_definer
"Usage: :find_verb_definer(object, verbname) => first ancestor whose verb_info answers; raises E_VERBNF";
{o, vname} = args;
set_task_perms(caller_perms());
what = o;
while (valid(what))
  if (`verb_info(what, vname) ! E_VERBNF => 0')
    return what;
  endif
  what = parent(what);
endwhile
raise(E_VERBNF);
.

@args #XXX:"summary_json" this none this
@chown #XXX:summary_json #2
@program #XXX:summary_json
"Usage: :summary_json(list of objects) => '{\"d\":[[num,name,[aliases]],...]}'";
"Object numbers are converted with toint() BEFORE encoding so generate_json never sees objnums.";
{objs} = args;
set_task_perms(caller_perms());
rows = {};
for o in (objs)
  name = `o.name ! ANY => ""';
  if (typeof(name) != STR)
    name = tostr(name);
  endif
  aliases = `o.aliases ! ANY => {}';
  if (typeof(aliases) != LIST)
    aliases = {};
  endif
  strs = {};
  for a in (aliases)
    if (typeof(a) == STR)
      strs = {@strs, a};
    endif
  endfor
  rows = {@rows, {toint(o), name, strs}};
endfor
return tostr("{\"d\":", this:json_encode(rows), "}");
.

@args #XXX:"json_encode" this none this
@chown #XXX:json_encode #2
@program #XXX:json_encode
"Usage: :json_encode(value) => minified JSON for strings, numbers, floats, objnums (bare ints), lists";
"Probes the generate_json() builtin once (cached in .use_generate_json: -1 unknown, 1 yes, 0 no).";
"Callers must convert objnums with toint() first; the OBJ branch below is only a safety net for";
"the fallback encoder (ToastStunt generate_json would encode objnums as \"#123\" strings).";
{value} = args;
use = this.use_generate_json;
if (use == -1)
  use = (`function_info("generate_json") ! ANY => 0') ? 1 | 0;
  `this.use_generate_json = use ! E_PERM';
endif
if (use == 1)
  return call_function("generate_json", value);
endif
t = typeof(value);
if (t == STR)
  return tostr("\"", strsub(strsub(value, "\\", "\\\\"), "\"", "\\\""), "\"");
elseif (t == OBJ)
  return tostr(toint(value));
elseif (t == LIST)
  parts = "";
  for item in (value)
    parts = tostr(parts, parts == "" ? "" | ",", this:json_encode(item));
  endfor
  return tostr("[", parts, "]");
elseif (t == ERR)
  return this:json_encode(tostr(value));
else
  return tostr(value);
endif
.

@args #XXX:"send_reply" this none this
@chown #XXX:send_reply #2
@program #XXX:send_reply
"Usage: :send_reply(session, reply-suffix, tag, json)";
"Hands the reply to the core MCP framework: session:send stamps the session";
"authentication key + the edgerunner-org-moo-query- prefix and emits the multiline";
"data* block. We only split the JSON into <=4000-char lines for that field; the";
"framework owns all wire framing. Do NOT hand-roll notify(\"#$#\"...) here.";
"session is the MCP session object (it owns :send, .authentication_key, .packages);";
"its .connection is the raw network connection the framework notifies.";
{session, suffix, tag, json} = args;
if (caller != this)
  raise(E_PERM);
endif
lines = {};
start = 1;
len = length(json);
while (start <= len)
  finish = min(start + 3999, len);
  lines = {@lines, json[start..finish]};
  start = finish + 1;
endwhile
session:send(suffix, this:parse_send_args(suffix, tag, lines));
.

@args #XXX:"send_error" this none this
@chown #XXX:send_error #2
@program #XXX:send_error
"Usage: :send_error(session, tag, error-list-from-except)";
"code = the MOO error name via toliteral (tostr would give the human message instead).";
"Routed through the core MCP framework (session:send) like send_reply, so the";
"session authentication key and package prefix are stamped by the framework.";
{session, tag, v} = args;
if (caller != this)
  raise(E_PERM);
endif
code = toliteral(v[1]);
msg = strsub(tostr(v[2]), "\"", "'");
session:send("error", this:parse_send_args("error", tag, code, msg));
.

"***finished***
