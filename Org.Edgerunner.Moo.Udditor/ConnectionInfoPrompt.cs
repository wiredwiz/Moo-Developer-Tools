﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Org.Edgerunner.Moo.Udditor;
public partial class ConnectionInfoPrompt : Form
{
    public ConnectionInfoPrompt()
    {
        InitializeComponent();
    }

    public string HostAddress
    {
        get => txtHost.Text;
        set => txtHost.Text = value;
    }

    public int HostPort
    {
        get => int.Parse(txtPort.Text);
        set => txtPort.Text = value.ToString();
    }

    private void btnConnect_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }
}