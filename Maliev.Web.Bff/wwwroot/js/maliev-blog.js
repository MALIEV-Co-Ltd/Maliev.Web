window.malievBlog = {
  downloadFile: function (href, filename) {
    const a = document.createElement('a');
    a.href = href;
    a.download = filename || '';
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }
};
