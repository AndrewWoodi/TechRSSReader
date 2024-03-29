
import { ArticlesState } from '../articles/state/articles.reducer';
import { BlogState } from '../blog/state/blog.reducer';

// Representation of the entire app state
export interface State {
  articles: ArticlesState;
  blogs: BlogState;
}
