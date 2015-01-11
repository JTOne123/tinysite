---
layout: master
---
<div>
<p>Table of Contents</p>
{{#books}}
<div class="book">
<ul>
{{#chapters}}
    <li class="chapter{{#childActive}} child-active{{/childActive}}{{#active}} active{{/active}}"><a href='{{document.url}}'>{{document.title}}</a></li>
<ul>
{{#children}}
    <li class="{{#if chapter}}chapter{{^}}page{{/if}}{{#childActive}} child-active{{/childActive}}{{#active}} active{{/active}}"><a href='{{document.url}}'>{{document.title}}</a></li>
{{/children}}
</ul>
</li>
{{/chapters}}
</ul>
</div>
{{/books}}
</div>
<div class="content">
{{{document.content}}}
</div>