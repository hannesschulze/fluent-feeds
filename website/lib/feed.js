import fs from 'fs';
import path from 'path';
import convert from 'xml-js';

const feedsDirectory = path.join(process.cwd(), 'public/blog/feeds');
const blogTitle = 'Fluent Feeds Blog';
const blogDescription = 'A blog about the Fluent Feeds app.';
const blogCopyright = '© 2022, the Fluent Feeds developers'
const blogUrl = 'https://hannesschulze.github.io/fluent-feeds/blog';
const blogFeedName = 'atom.xml';
const blogFeedUrl = `${blogUrl}/feeds/${blogFeedName}`;

export async function generateFeeds(posts) {
  const atom = convert.js2xml({
    _declaration: {
      _attributes: {
        version: '1.0',
        encoding: 'utf-8'
      }
    },
    feed: {
      _attributes: {
        xmlns: 'http://www.w3.org/2005/Atom'
      },
      title: {
        _text: blogTitle
      },
      subtitle: {
        _text: blogDescription
      },
      link: [
        {
          _attributes: {
            rel: 'alternate',
            type: 'text/html',
            href: blogUrl
          }
        },
        {
          _attributes: {
            rel: 'self',
            type: 'application/atom+xml',
            href: blogFeedUrl
          }
        }
      ],
      id: {
        _text: blogUrl
      },
      updated: {
        _text: new Date().toISOString()
      },
      rights: {
        _text: blogCopyright
      },
      entry: posts.map(post => {
        const url = `${blogUrl}/posts/${post.slug}`;
        const publishedTimestamp = new Date(post.publishedTimestamp);
        const modifiedTimestamp = post.modifiedTimestamp ? new Date(post.modifiedTimestamp) : publishedTimestamp;
        return {
          title: {
            _cdata: post.title
          },
          link: {
            _attributes: {
              rel: 'alternate',
              type: 'text/html',
              href: url
            }
          },
          id: {
            _text: url
          },
          published: {
            _text: publishedTimestamp.toISOString()
          },
          updated: {
            _text: modifiedTimestamp.toISOString()
          },
          author: {
            name: {
              _text: post.author
            }
          },
          summary: {
            _attributes: {
              type: 'text'
            },
            _cdata: post.summary
          },
          content: {
            _attributes: {
              type: 'html',
              'xml:base': url,
              'xml:lang': 'en'
            },
            _cdata: post.content
          }
        };
      })
    }
  }, {
    compact: true,
    ignoreComment: true,
    spaces: 2
  });

  await fs.promises.mkdir(feedsDirectory, { recursive: true });
  await fs.promises.writeFile(path.join(feedsDirectory, blogFeedName), atom);
}
