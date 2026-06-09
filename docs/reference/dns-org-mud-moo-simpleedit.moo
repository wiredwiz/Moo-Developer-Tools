;;#230.("foo") = 1
;;#230.("v_filter_in") = {#230, "verbcode_external_to_internal"}
;;#230.("v_filter_out") = {#230, "verbcode_internal_to_external"}
;;#230.("version_range") = {"1.0", "1.0"}
;;#230.("messages_in") = {{"set", {"reference", "type", "content"}}}
;;#230.("messages_out") = {{"content", {"reference", "name", "type", "content"}}}
;;#230.("aliases") = {"dns-org-mud-moo-simpleedit"}
;;#230.("description") = {"A very simple local editing protocol.", "", "S->C #$#dns-org-mud-moo-simpleedit-content (reference, name, type, content*)", "", "Reference is the tag used when sending back to the server.", "User may be allowed to edit it (i.e., save this same text into a", "different property), with possibility of disastrous results.", "", "Name is a human-readable name for the info, suitable for window title,", "buffer name, etc.", "", "Type is one of the following (for version 1.0):", " * string", " * string-list", " * moo-code", "", "Content is the content interpreted according to the type", "info given.  It's multiline (hence the *).", "", "clients that don't provide special support for moo-code editing can", "treat moo-code identically to string-list.", "", "C->S #$#dns-org-mud-moo-simpleedit-set (reference, content*, type)", "", "reference, content, and type are as above.  This is the message sent by the client to set when the user 'saves' the value.  Note this does not necessarily save the value.  Errors such as lack of permission to set the given reference or moo-code compliation errors may prevent it.  It is expected that the server will tell the user this (in the in-band text stream).  ", "", "Clients will probably want to provide a way to just send without closing the window, buffer, etc for this reason.", "", "", "JHCore implementation notes", "", "JHCore currently understands several different kinds of (local) editing sessions:", "handled by $verb_editor:", "   * verb editing", "handled by $note_editor:", "   * list of strings editing for notes and properties", "handled by both $note_editor and $list_editor(?):", "   * value editing for properties", "handled by $mail_editor:", "   * sending a mail message", "", "@edit uses a semi-complicated system to determine (a) what the user is trying to edit and (b) how to edit it.   ", "", "So it looks like the critical things to modify are $generic_editor:invoke_local_editor, $note_editor:local_editing_info, $verb_editor:local_editing_info, and $mail_editor:local_editing_info.  ", "", "The current return path for locally edited stuff (in the core) appears to be:", "  @program", "  @set-note-text", "  @set-note-value", "  @@sendmail", "", ".v_filter_in / .v_filter_out are hooks called when receiving/sending verb code.  JHCore supports a '// comment' syntax in verbs and conversion is made from '// blah' to '\"blah\";' by using the verbs :verbcode_external_to_internal / :verbcode_internal_to_external.", "", ".v_filter_in / .v_filter_out are both 2 element lists containing an object id and a verb.  the JHCore configuration is:", "", "    this.v_filter_in = { this, \"verbcode_external_to_internal\" }", "    this.v_filter_out = { this, \"verbcode_internal_to_external\" }", "", "Non JHCore installations might choose to set .v_filter_in / .v_filter_out to false (0, \"\" or {}) which leaves code lines unfiltered.", "", ""}
;;#230.("object_size") = {16920, 1655798400}
;;#230.("last_modified") = 1656127950

@args #230:"send_content" this none this
@chown #230:send_content #2
@program #230:send_content
"Usage:  :send_content()";
"";
if ($perm_utils:controls(caller_perms(), args[1]))
  pass(@args);
else
  raise(E_PERM);
endif
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"handle_set" this none this
@chown #230:handle_set #2
@program #230:handle_set
"Usage:  :handle_set(session, reference, type, content)";
"";
{session, reference, type, content} = args;
if (caller != this)
  raise(E_PERM);
endif
set_task_perms(session.connection);
try
  if (type == "moo-code")
    rval = this:edit_set_program(reference, content);
  elseif (reference == "sendmail")
    rval = this:edit_sendmail(reference, content);
  else
    rval = this:edit_set_note_value(reference, type, content);
  endif
  player:notify_lines((typeof(rval) == LIST) ? rval | {rval});
except v (ANY)
  player:notify_lines((typeof(v[2]) == LIST) ? v[2] | {v[2]});
  "   player:notify_lines($code_utils:format_traceback(v[4], v[2]));";
endtry
"Last modified Sat Jun  3 20:31:25 2000 GMT by MCP_Master, #213@EdgeRunner.";
"Last modified Tue Jul  4 21:25:38 2000 EST by Dakkon, #100@EdgeRunner.";
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"edit_set_program" this none this
@chown #230:edit_set_program #2
@program #230:edit_set_program
{reference, lines} = args;
set_task_perms(caller_perms());
args = $string_utils:words(reference);
punt = 1;
if (!(spec = $code_utils:parse_verbref(args[1])))
  raise(E_INVARG, "Invalid reference: " + reference);
elseif ($command_utils:object_match_failed(object = player:my_match_object(spec[1]), spec[1]))
  return;
elseif ($string_utils:is_numeric(spec[2]))
  "numeric verbref";
  if ((verbname = $code_utils:tonum(spec[2])) == E_TYPE)
    raise(E_INVARG, "Invalid verb number");
  elseif (length(args) > 1)
    raise(E_INVARG, "Invalid reference: " + reference);
  elseif ((verbname < 1) || `verbname > length(verbs(object)) ! E_PERM => 0')
    raise(E_INVARG, "Verb number out of range.");
  else
    argspec = 0;
    punt = 0;
  endif
elseif (typeof(argspec = $code_utils:parse_argspec(@listdelete(args, 1))) != LIST)
  raise(E_INVARG, tostr(argspec));
elseif (argspec[2])
  raise(E_INVARG, $string_utils:from_list(argspec[2], " ") + "??");
elseif (length(argspec = argspec[1]) in {1, 2})
  raise(E_INVARG, {"Missing preposition", "Missing iobj specification"}[length(argspec)]);
else
  punt = 0;
  verbname = spec[2];
  if (index(verbname, "*") > 1)
    verbname = strsub(verbname, "*", "");
  endif
endif
"...";
"...if we have an argspec, we'll need to reset verbname...";
"...";
if (punt)
elseif (argspec)
  named = argspec[4..min(5, $)];
  argspec = argspec[1..3];
  if (!(argspec[2] in {"none", "any"}))
    argspec[2] = $code_utils:full_prep(argspec[2]);
  endif
  loc = $code_utils:find_verb_named_1_based(object, verbname);
  while (loc && (`verb_args(object, loc) ! E_PERM' != argspec))
    loc = $code_utils:find_verb_named_1_based(object, verbname, loc + 1);
  endwhile
  if (loc)
    verbname = loc;
  else
    punt = "...can't find it....";
    raise(E_INVARG, "That object has no verb matching that name + args.");
  endif
else
  named = {};
  loc = (typeof(verbname) == NUM) ? verbname | 0;
endif
if (!punt)
  try
    info = $verb_info(object, verbname);
  except e (ANY)
    if (e[1] == E_VERBNF)
      raise(E_INVARG, "That object does not have that verb definition.");
    else
      raise(E_INVARG, tostr(e[2]));
    endif
    punt = 1;
  endtry
  if (!punt)
    aliases = info[3];
    if (!loc)
      loc = aliases in (verbs(object) || {});
    endif
  endif
endif
if (punt)
  return;
else
  "filter the verb?";
  if (this.v_filter_in)
    lines = this.v_filter_in[1]:(this.v_filter_in[2])(lines);
  endif
  if (0 && named)
    "Disabled: We want to see all of the code in the verb and not in the title.";
    code = $code_utils:split_verb_code(lines);
    lines = {@code[1], @$code_utils:named_args_to_code(named), @code[2]};
  endif
  try
    result = $set_verb_code(object, verbname, lines);
  except e (ANY)
    result = e[2] + " ";
    "just in case some idiot throws an error with an empty string";
  endtry
  what = $string_utils:nn(object);
  if (result)
    if (typeof(result) == STR)
      return {"Error programming " + what, result, "Verb not programmed."};
    else
      return {"Error programming " + what, @result, tostr(length(result), " error(s)."), "Verb not programmed."};
    endif
  else
    return {"0 errors.", "Verb programmed."};
  endif
endif
"Last modified Sat Jun  3 21:03:48 2000 GMT by MCP_Master, #213@EdgeRunner.";
"Last modified Tue Jul  4 21:25:19 2000 EST by Dakkon, #100@EdgeRunner.";
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"edit_sendmail" this none this
@chown #230:edit_sendmail #2
@program #230:edit_sendmail
"See $player:@@sendmail";
set_task_perms(caller_perms());
{reference, msg} = args;
end_head = ("" in msg) || (length(msg) + 1);
subject = "";
replyto = "";
rcpts = {};
body = msg[end_head + 1..length(msg)];
for i in [1..end_head - 1]
  line = msg[i];
  if (index(line, "Subject:") == 1)
    subject = $string_utils:trim(line[9..length(line)]);
  elseif (index(line, "To:") == 1)
    if (!(rcpts = $mail_agent:parse_address_field(line)))
      player:notify("No recipients found in To: line");
      return;
    endif
  elseif (index(line, "Reply-to:") == 1)
    if ((!(replyto = $mail_agent:parse_address_field(line))) && $string_utils:trim(line[10..length(line)]))
      player:notify("No address found in Reply-to: line");
      return;
    endif
  elseif (i = index(line, ":"))
    player:notify(tostr("Unknown header \"", line[1..i], "\""));
    return;
  else
    player:notify("Blank line must separate headers from body.");
    return;
  endif
endfor
if (!rcpts)
  player:notify("No To: line found.");
elseif (!(subject || body))
  player:notify("Blank message not sent.");
else
  player:notify("Sending...");
  result = $mail_agent:send_message(player, rcpts, replyto ? subject | {subject, replyto}, body);
  if (e = result && result[1])
    if (length(result) == 1)
      player:notify("Mail actually went to no one.");
    else
      player:notify(tostr("Mail actually went to ", $mail_agent:name_list(@listdelete(result, 1)), "."));
    endif
  else
    player:notify(tostr((typeof(e) == ERR) ? e | ("Bogus recipients:  " + $string_utils:from_list(result[2]))));
    player:notify("Mail not sent.");
  endif
endif
return {};
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"edit_set_note_value" this none this
@chown #230:edit_set_note_value #2
@program #230:edit_set_note_value
set_task_perms(caller_perms());
{reference, type, content} = args;
"reference format == [str|val]:#xx[.pname]";
if (!match(reference, "^%(str%|val%):.+"))
  return {"Malformed reference: " + reference};
else
  vtype = reference[1..3];
  reference = reference[5..$];
  if (((vtype == "str") && (type == "string")) && (length(content) <= 1))
    text = content ? content[1] | "";
  elseif (vtype == "val")
    text = {};
    for x in [1..length(content)]
      $sin(0);
      value = $string_utils:to_value(content[x]);
      if (value[1] != 1)
        return {tostr("Error on line ", x, ":  ", value[2]), "Value not saved."};
      else
        text = {@text, value[2]};
      endif
    endfor
  else
    text = content;
  endif
endif
if (spec = $code_utils:parse_propref(reference))
  o = $code_utils:toobj(spec[1]);
  p = spec[2];
  if (typeof(o) == OBJ)
    if ($object_utils:has_callable_verb(o, setter = "set_" + p))
      e = o:(setter)(text);
    else
      e = o.(p) = text;
    endif
  else
    return {"Malformed reference: You must supply an object number."};
  endif
  if (typeof(e) == ERR)
    raise(e, tostr("Error: ", e));
  else
    return tostr("Set ", p, " property of ", o.name, " (", o, ").");
  endif
elseif (typeof(note = $code_utils:toobj(argstr)) == OBJ)
  o = note;
  e = note:set_text(text);
  if (typeof(e) == ERR)
    return {tostr("Error: ", e)};
  else
    return tostr("Set text of ", o.name, ".");
  endif
else
  raise(E_INVARG, tostr("Error: Malformed argument to ", verb, ": ", argstr));
endif
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"verbcode_external_to_internal" this none this
@program #230:verbcode_external_to_internal
"Charter: given a block of verb code lines from the user, transform it into code ready to be passed to set_verb_code().  In particular, reverse any transformation made by :verbcode_internal_to_external.";
"This version transforms `// foo' comments to `\"foo\";' comments.";
lines = args[1];
new_comments = player:prog_option("//_comments");
if (!new_comments)
  return lines;
endif
newlines = {};
for line in (lines)
  mat = match(line, "^ *// ?%(.*%)$");
  if (mat)
    comment = substitute("%1", mat);
    out = $code_utils:commentify({comment});
    newlines = {@newlines, out[1]};
  else
    newlines = {@newlines, line};
  endif
endfor
return newlines;
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

@args #230:"verbcode_internal_to_external" this none this
@program #230:verbcode_internal_to_external
"Charter: given a block of verb code from the verb_code() primitive, transform it into its external representation to be presented to the user.";
"This version transforms `\"foo\";' comments to `// foo' comments.";
lines = args[1];
new_comments = player:prog_option("//_comments");
if (!new_comments)
  return lines;
endif
newlines = {};
for line in (lines)
  mat = match(line, "^%( *%)%(\".*\";%)$");
  if (mat)
    blanks = substitute("%1", mat);
    comment = substitute("%2", mat);
    uncommented = $code_utils:uncommentify({comment});
    out = (blanks + "// ") + uncommented[1];
    newlines = {@newlines, out};
  else
    newlines = {@newlines, line};
  endif
endfor
return newlines;
"Last modified Fri Jun 24 23:32:30 2022 EDT by Wizard, #2@EdgeRunner.";
.

"***finished***