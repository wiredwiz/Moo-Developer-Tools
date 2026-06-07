"This verb demonstrates every syntax token category for theme preview.";
"It is sample code only and is never executed.";
counter = 0;
limit = 10;
ratio = 3.14;
flag = E_PERM;
greeting = "Hello, world!";
container = #123;
core = $string_utils;
items = {1, 2, 3, "four", #5};
lookup = [1 -> "one", 2 -> "two"];
if (player.name == greeting && counter < limit)
  this:tell(tostr("Counting: ", counter));
  for item in (items)
    counter = counter + 1;
    notify(player, item);
  endfor
elseif (counter > limit || flag == E_NONE)
  result = this:process(args, dobj, iobj);
  player.location = container;
else
  return $command_utils:object_match(argstr);
endif
try
  value = container.contents[1];
  total = (counter * 2) - (limit / ratio) % 4;
except err (ANY)
  this:handle_error(err, verb, caller);
finally
  fork (5)
    this:cleanup();
  endfork
endtry
while (counter > 0)
  counter = counter - 1;
endwhile
